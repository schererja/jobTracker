namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to get a presigned URL for uploading an attachment.
/// </summary>
public class PresignUploadRequest
{
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long SizeBytes { get; set; }
  public string Type { get; set; } = string.Empty; // e.g., "Resume", "CoverLetter"
}
