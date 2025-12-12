using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;
using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Functions;

public class InterviewFunctions
{
  private readonly IApplicationRepository _appRepo;
  private readonly IInterviewRepository _interviewRepo;
  private readonly IIdentityService _identity;

  public InterviewFunctions(
      IApplicationRepository appRepo,
      IInterviewRepository interviewRepo,
      IIdentityService identity)
  {
    _appRepo = appRepo;
    _interviewRepo = interviewRepo;
    _identity = identity;
  }

  [Function("ListInterviews")]
  public async Task<HttpResponseData> ListInterviews(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}/interviews")] HttpRequestData req,
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

      var pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? ps : 50;
      var continuationToken = req.Query["continuationToken"];

      var (items, token) = await _interviewRepo.ListAsync(appId, pageSize, continuationToken, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(new PaginatedResponse<InterviewEvent>
      {
        Items = items,
        ContinuationToken = token,
        Count = items.Count
      }, cancellationToken: ct);

      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to list interviews", ex.Message);
    }
  }

  [Function("CreateInterview")]
  public async Task<HttpResponseData> CreateInterview(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId}/interviews")] HttpRequestData req,
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

      var createRequest = await req.ReadFromJsonAsync<CreateInterviewRequest>();
      if (createRequest == null)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

      var interview = new InterviewEvent
      {
        InterviewId = Guid.NewGuid(),
        ApplicationId = appId,
        Date = createRequest.Date,
        Interviewer = createRequest.Interviewer,
        Type = createRequest.Type,
        Notes = createRequest.Notes
      };

      var created = await _interviewRepo.CreateAsync(interview, ct);

      var response = req.CreateResponse(HttpStatusCode.Created);
      await response.WriteAsJsonAsync(created, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to create interview", ex.Message);
    }
  }

  [Function("GetInterview")]
  public async Task<HttpResponseData> GetInterview(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}/interviews/{interviewId}")] HttpRequestData req,
      string applicationId,
      string interviewId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId) || !Guid.TryParse(interviewId, out var intId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid ID format");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var interview = await _interviewRepo.GetByIdAsync(intId, appId, ct);
      if (interview == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Interview not found");

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(interview, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to get interview", ex.Message);
    }
  }

  [Function("UpdateInterview")]
  public async Task<HttpResponseData> UpdateInterview(
      [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{applicationId}/interviews/{interviewId}")] HttpRequestData req,
      string applicationId,
      string interviewId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId) || !Guid.TryParse(interviewId, out var intId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid ID format");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var interview = await _interviewRepo.GetByIdAsync(intId, appId, ct);
      if (interview == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Interview not found");

      var updateRequest = await req.ReadFromJsonAsync<UpdateInterviewRequest>();
      if (updateRequest != null)
      {
        if (updateRequest.Date.HasValue)
          interview.Date = updateRequest.Date.Value;
        if (!string.IsNullOrEmpty(updateRequest.Interviewer))
          interview.Interviewer = updateRequest.Interviewer;
        if (updateRequest.Type.HasValue)
          interview.Type = updateRequest.Type.Value;
        if (updateRequest.Notes != null)
          interview.Notes = updateRequest.Notes;
      }

      var updated = await _interviewRepo.UpdateAsync(interview, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(updated, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to update interview", ex.Message);
    }
  }

  [Function("DeleteInterview")]
  public async Task<HttpResponseData> DeleteInterview(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}/interviews/{interviewId}")] HttpRequestData req,
      string applicationId,
      string interviewId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId) || !Guid.TryParse(interviewId, out var intId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid ID format");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      await _interviewRepo.DeleteAsync(intId, appId, ct);

      return req.CreateResponse(HttpStatusCode.NoContent);
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to delete interview", ex.Message);
    }
  }

  private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    response.Headers.Add("Content-Type", "application/json");
    return response;
  }
}
