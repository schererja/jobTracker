using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Repository interface for status history operations.
/// </summary>
public interface IStatusHistoryRepository
{
  /// <summary>
  /// Append a status history entry.
  /// </summary>
  Task<StatusHistory> AppendAsync(StatusHistory history, CancellationToken ct = default);

  /// <summary>
  /// List status history for an application with pagination.
  /// </summary>
  Task<(List<StatusHistory> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default);
}
