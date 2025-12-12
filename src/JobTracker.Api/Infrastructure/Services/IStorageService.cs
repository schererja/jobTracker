using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Infrastructure.Services;

/// <summary>
/// Service for managing storage operations (file uploads/downloads).
/// </summary>
public interface IStorageService
{
  /// <summary>
  /// Generate a presigned/SAS URL for uploading a file.
  /// </summary>
  Task<PresignUploadResponse> GenerateUploadUrlAsync(
      Guid userId,
      Guid applicationId,
      Guid attachmentId,
      string fileName,
      string contentType,
      CancellationToken ct = default);

  /// <summary>
  /// Generate a presigned/SAS URL for downloading a file.
  /// </summary>
  Task<PresignDownloadResponse> GenerateDownloadUrlAsync(
      string storagePath,
      string fileName,
      CancellationToken ct = default);

  /// <summary>
  /// Delete a file from storage.
  /// </summary>
  Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
