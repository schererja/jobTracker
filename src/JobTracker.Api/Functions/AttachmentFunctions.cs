using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;
using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Functions;

public class AttachmentFunctions
{
  private readonly IApplicationRepository _appRepo;
  private readonly IAttachmentRepository _attachmentRepo;
  private readonly IStorageService _storageService;
  private readonly IIdentityService _identity;

  public AttachmentFunctions(
      IApplicationRepository appRepo,
      IAttachmentRepository attachmentRepo,
      IStorageService storageService,
      IIdentityService identity)
  {
    _appRepo = appRepo;
    _attachmentRepo = attachmentRepo;
    _storageService = storageService;
    _identity = identity;
  }

  [Function("PresignUpload")]
  public async Task<HttpResponseData> PresignUpload(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId}/attachments/presign-upload")] HttpRequestData req,
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

      var presignRequest = await req.ReadFromJsonAsync<PresignUploadRequest>();
      if (presignRequest == null)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

      // Validate file size (max 25 MB)
      if (presignRequest.SizeBytes > 25 * 1024 * 1024)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "File size exceeds 25 MB limit");

      var attachmentId = Guid.NewGuid();
      var presignedUrl = await _storageService.GenerateUploadUrlAsync(
          userId,
          appId,
          attachmentId,
          presignRequest.FileName,
          presignRequest.ContentType,
          ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(presignedUrl, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to generate upload URL", ex.Message);
    }
  }

  [Function("ConfirmUpload")]
  public async Task<HttpResponseData> ConfirmUpload(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId}/attachments")] HttpRequestData req,
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

      var confirmRequest = await req.ReadFromJsonAsync<ConfirmUploadRequest>();
      if (confirmRequest == null)
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

      // In real implementation, verify file exists in storage
      var attachment = new Attachment
      {
        AttachmentId = confirmRequest.AttachmentId,
        ApplicationId = appId,
        Type = "Document",
        FileName = "uploaded-file",
        ContentType = "application/octet-stream",
        SizeBytes = 0,
        StoragePath = $"{userId}/{appId}/{confirmRequest.AttachmentId}",
        UploadedAt = DateTime.UtcNow
      };

      var created = await _attachmentRepo.CreateAsync(attachment, ct);

      var response = req.CreateResponse(HttpStatusCode.Created);
      await response.WriteAsJsonAsync(created, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to confirm upload", ex.Message);
    }
  }

  [Function("ListAttachments")]
  public async Task<HttpResponseData> ListAttachments(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}/attachments")] HttpRequestData req,
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

      var (items, token) = await _attachmentRepo.ListAsync(appId, pageSize, continuationToken, ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(new PaginatedResponse<Attachment>
      {
        Items = items,
        ContinuationToken = token,
        Count = items.Count
      }, cancellationToken: ct);

      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to list attachments", ex.Message);
    }
  }

  [Function("PresignDownload")]
  public async Task<HttpResponseData> PresignDownload(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}/attachments/{attachmentId}/presign-download")] HttpRequestData req,
      string applicationId,
      string attachmentId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId) || !Guid.TryParse(attachmentId, out var attId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid ID format");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var attachment = await _attachmentRepo.GetByIdAsync(attId, appId, ct);
      if (attachment == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Attachment not found");

      var presignedUrl = await _storageService.GenerateDownloadUrlAsync(
          attachment.StoragePath,
          attachment.FileName,
          ct);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(presignedUrl, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to generate download URL", ex.Message);
    }
  }

  [Function("DeleteAttachment")]
  public async Task<HttpResponseData> DeleteAttachment(
      [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}/attachments/{attachmentId}")] HttpRequestData req,
      string applicationId,
      string attachmentId,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();

      if (!Guid.TryParse(applicationId, out var appId) || !Guid.TryParse(attachmentId, out var attId))
        return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid ID format");

      var app = await _appRepo.GetByIdAsync(appId, userId, ct);
      if (app == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Application not found");

      var attachment = await _attachmentRepo.GetByIdAsync(attId, appId, ct);
      if (attachment == null)
        return CreateErrorResponse(req, HttpStatusCode.NotFound, "Attachment not found");

      await _storageService.DeleteAsync(attachment.StoragePath, ct);
      await _attachmentRepo.DeleteAsync(attId, appId, ct);

      return req.CreateResponse(HttpStatusCode.NoContent);
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to delete attachment", ex.Message);
    }
  }

  private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    response.Headers.Add("Content-Type", "application/json");
    return response;
  }
}
