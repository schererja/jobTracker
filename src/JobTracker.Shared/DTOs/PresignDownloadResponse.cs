namespace JobTracker.Shared.DTOs;

/// <summary>
/// Response containing presigned URL for downloading an attachment.
/// </summary>
public class PresignDownloadResponse
{
  public string DownloadUrl { get; set; } = string.Empty;
  public string FileName { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
}
