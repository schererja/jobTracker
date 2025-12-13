namespace JobTracker.Shared.DTOs;

/// <summary>
/// Response containing presigned URL and metadata for uploading.
/// </summary>
public class PresignUploadResponse
{
  public string UploadUrl { get; set; } = string.Empty;
  public string StoragePath { get; set; } = string.Empty;
  public Dictionary<string, string>? Headers { get; set; }
  public Guid AttachmentId { get; set; }
}
