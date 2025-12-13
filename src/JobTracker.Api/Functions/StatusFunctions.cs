using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;
using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Functions;

public class StatusFunctions
{
  private readonly IApplicationRepository _appRepo;
  private readonly IStatusHistoryRepository _historyRepo;
  private readonly IIdentityService _identity;

  public StatusFunctions(
      IApplicationRepository appRepo,
      IStatusHistoryRepository historyRepo,
      IIdentityService identity)
  {
    _appRepo = appRepo;
    _historyRepo = historyRepo;
    _identity = identity;
  }

  [Function("UpdateApplicationStatus")]
  public async Task<HttpResponseData> UpdateApplicationStatus(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId}/status")] HttpRequestData req,
      string applicationId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId(req);

      if (!Guid.TryParse(applicationId, out var appId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid application ID");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var statusRequest = await req.ReadFromJsonAsync<UpdateStatusRequest>();
      if (statusRequest == null)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

      var oldStatus = app.Status;
      app.Status = statusRequest.NewStatus;

      // Create status history entry
      var history = new StatusHistory
      {
        HistoryId = Guid.NewGuid(),
        ApplicationId = appId,
        OldStatus = oldStatus,
        NewStatus = statusRequest.NewStatus,
        ChangedAt = DateTime.UtcNow
      };
      history.id = history.HistoryId.ToString();

      await _historyRepo.AppendAsync(history, ct);
      var updated = await _appRepo.UpdateAsync(app, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(updated, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to update status", ex.Message);
    }
  }

  [Function("GetStatusHistory")]
  public async Task<HttpResponseData> GetStatusHistory(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}/status-history")] HttpRequestData req,
      string applicationId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId(req);

      if (!Guid.TryParse(applicationId, out var appId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid application ID");

      // Verify app belongs to user
      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? ps : 50;
      var continuationToken = req.Query["continuationToken"];

      var (items, token) = await _historyRepo.ListAsync(appId, pageSize, continuationToken, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(new PaginatedResponse<StatusHistory>
      {
        Items = items,
        ContinuationToken = token,
        Count = items.Count
      }, cancellationToken: ct);

      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to get status history", ex.Message);
    }
  }

  private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    response.Headers.Add("Content-Type", "application/json");
    return response;
  }
}
