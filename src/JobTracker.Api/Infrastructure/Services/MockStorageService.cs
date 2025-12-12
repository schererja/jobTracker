using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Infrastructure.Services;

/// <summary>
/// Mock implementation of IStorageService for development.
/// In production, implement with Azure Blob Storage.
/// </summary>
public class MockStorageService : IStorageService
{
  public Task<PresignUploadResponse> GenerateUploadUrlAsync(
      Guid userId,
      Guid applicationId,
      Guid attachmentId,
      string fileName,
      string contentType,
      CancellationToken ct = default)
  {
    var storagePath = $"{userId}/{applicationId}/{attachmentId}-{DateTime.UtcNow:yyyyMMddHHmmss}-{fileName}";
    var uploadUrl = $"https://mock-storage.blob.core.windows.net/uploads?path={Uri.EscapeDataString(storagePath)}";

    return Task.FromResult(new PresignUploadResponse
    {
      UploadUrl = uploadUrl,
      StoragePath = storagePath,
      AttachmentId = attachmentId,
      Headers = new Dictionary<string, string>
            {
                { "Content-Type", contentType },
                { "x-ms-blob-type", "BlockBlob" }
            }
    });
  }

  public Task<PresignDownloadResponse> GenerateDownloadUrlAsync(
      string storagePath,
      string fileName,
      CancellationToken ct = default)
  {
    var downloadUrl = $"https://mock-storage.blob.core.windows.net/downloads?path={Uri.EscapeDataString(storagePath)}";

    return Task.FromResult(new PresignDownloadResponse
    {
      DownloadUrl = downloadUrl,
      FileName = fileName,
      ExpiresAt = DateTime.UtcNow.AddHours(1)
    });
  }

  public Task DeleteAsync(string storagePath, CancellationToken ct = default)
  {
    // Mock delete - no-op
    return Task.CompletedTask;
  }
}
