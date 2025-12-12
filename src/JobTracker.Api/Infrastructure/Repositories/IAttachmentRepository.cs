using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Repository interface for attachment operations.
/// </summary>
public interface IAttachmentRepository
{
  /// <summary>
  /// Create an attachment record.
  /// </summary>
  Task<Attachment> CreateAsync(Attachment attachment, CancellationToken ct = default);

  /// <summary>
  /// Get an attachment by ID.
  /// </summary>
  Task<Attachment?> GetByIdAsync(Guid attachmentId, Guid applicationId, CancellationToken ct = default);

  /// <summary>
  /// List attachments for an application with pagination.
  /// </summary>
  Task<(List<Attachment> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default);

  /// <summary>
  /// Delete an attachment.
  /// </summary>
  Task DeleteAsync(Guid attachmentId, Guid applicationId, CancellationToken ct = default);
}
