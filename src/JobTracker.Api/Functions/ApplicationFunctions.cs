using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;
using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Functions;

public class ApplicationFunctions
{
  private readonly IApplicationRepository _appRepo;
  private readonly IUserRepository _userRepo;
  private readonly IIdentityService _identity;

  public ApplicationFunctions(
      IApplicationRepository appRepo,
      IUserRepository userRepo,
      IIdentityService identity)
  {
    _appRepo = appRepo;
    _userRepo = userRepo;
    _identity = identity;
  }

  [Function("ListApplications")]
  public async Task<HttpResponseData> ListApplications(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications")] HttpRequestData req,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      var query = req.Url.Query;
      var statusStr = req.Query["status"];
      var company = req.Query["company"];
      var source = req.Query["source"];
      var appliedFrom = req.Query["appliedFrom"];
      var appliedTo = req.Query["appliedTo"];
      var searchQuery = req.Query["q"];
      var pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? ps : 20;
      var continuationToken = req.Query["continuationToken"];

      ApplicationStatus[]? statuses = null;
      if (!string.IsNullOrEmpty(statusStr))
      {
        statuses = statusStr.Split(',')
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => Enum.Parse<ApplicationStatus>(s))
            .ToArray();
      }

      ApplicationSource? sourceEnum = null;
      if (!string.IsNullOrEmpty(source) && Enum.TryParse<ApplicationSource>(source, out var s))
      {
        sourceEnum = s;
      }

      DateTime? appliedFromDate = string.IsNullOrEmpty(appliedFrom) ? null : DateTime.Parse(appliedFrom);
      DateTime? appliedToDate = string.IsNullOrEmpty(appliedTo) ? null : DateTime.Parse(appliedTo);

      var result = await _appRepo.ListAsync(
          userId,
          statuses,
          company,
          sourceEnum,
          appliedFromDate,
          appliedToDate,
          searchQuery,
          pageSize,
          continuationToken,
          ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(result, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to list applications", ex.Message);
    }
  }

  [Function("CreateApplication")]
  public async Task<HttpResponseData> CreateApplication(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications")] HttpRequestData req,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();
      var createRequest = await req.ReadFromJsonAsync<CreateApplicationRequest>();

      if (createRequest == null)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

      var app = new JobApplication
      {
        ApplicationId = Guid.NewGuid(),
        UserId = userId,
        Company = createRequest.Company,
        RoleTitle = createRequest.RoleTitle,
        Location = createRequest.Location,
        SalaryRange = createRequest.SalaryRange,
        AppliedDate = createRequest.AppliedDate,
        Status = createRequest.Status,
        Source = createRequest.Source,
        Url = createRequest.Url,
        ResumeUsed = createRequest.ResumeUsed,
        Notes = createRequest.Notes
      };

      var created = await _appRepo.CreateAsync(app, ct);

      var response = req.CreateResponse(HttpStatusCode.Created);
      await response.WriteAsJsonAsync(created, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to create application", ex.Message);
    }
  }

  [Function("GetApplication")]
  public async Task<HttpResponseData> GetApplication(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}")] HttpRequestData req,
      string applicationId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid application ID");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(app, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to get application", ex.Message);
    }
  }

  [Function("UpdateApplication")]
  public async Task<HttpResponseData> UpdateApplication(
      [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{applicationId}")] HttpRequestData req,
      string applicationId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid application ID");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var updateRequest = await req.ReadFromJsonAsync<UpdateApplicationRequest>();
      if (updateRequest != null)
      {
        if (!string.IsNullOrEmpty(updateRequest.Company))
          app.Company = updateRequest.Company;
        if (!string.IsNullOrEmpty(updateRequest.RoleTitle))
          app.RoleTitle = updateRequest.RoleTitle;
        if (updateRequest.Location != null)
          app.Location = updateRequest.Location;
        if (updateRequest.SalaryRange != null)
          app.SalaryRange = updateRequest.SalaryRange;
        if (updateRequest.Url != null)
          app.Url = updateRequest.Url;
        if (updateRequest.ResumeUsed != null)
          app.ResumeUsed = updateRequest.ResumeUsed;
        if (updateRequest.Notes != null)
          app.Notes = updateRequest.Notes;
      }

      var updated = await _appRepo.UpdateAsync(app, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(updated, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to update application", ex.Message);
    }
  }

  [Function("DeleteApplication")]
  public async Task<HttpResponseData> DeleteApplication(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}")] HttpRequestData req,
      string applicationId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid application ID");

      await _appRepo.DeleteAsync(appId, userId, ct);

      return req.CreateResponse(HttpStatusCode.NoContent);
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to delete application", ex.Message);
    }
  }

  private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    response.Headers.Add("Content-Type", "application/json");
    return response;
  }
}
