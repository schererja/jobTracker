using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Repository interface for interview event operations.
/// </summary>
public interface IInterviewRepository
{
  /// <summary>
  /// Create a new interview event.
  /// </summary>
  Task<InterviewEvent> CreateAsync(InterviewEvent interview, CancellationToken ct = default);

  /// <summary>
  /// Get an interview by ID.
  /// </summary>
  Task<InterviewEvent?> GetByIdAsync(Guid interviewId, Guid applicationId, CancellationToken ct = default);

  /// <summary>
  /// List interviews for an application with pagination.
  /// </summary>
  Task<(List<InterviewEvent> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default);

  /// <summary>
  /// Update an interview event.
  /// </summary>
  Task<InterviewEvent> UpdateAsync(InterviewEvent interview, CancellationToken ct = default);

  /// <summary>
  /// Delete an interview event.
  /// </summary>
  Task DeleteAsync(Guid interviewId, Guid applicationId, CancellationToken ct = default);
}
