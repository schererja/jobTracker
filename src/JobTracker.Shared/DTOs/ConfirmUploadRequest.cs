namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to confirm an attachment upload and persist metadata.
/// </summary>
public class ConfirmUploadRequest
{
  public Guid AttachmentId { get; set; }
}
