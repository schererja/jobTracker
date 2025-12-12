using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Repository interface for job application operations.
/// </summary>
public interface IApplicationRepository
{
  /// <summary>
  /// Create a new job application.
  /// </summary>
  Task<JobApplication> CreateAsync(JobApplication application, CancellationToken ct = default);

  /// <summary>
  /// Get an application by ID, ensuring user ownership.
  /// </summary>
  Task<JobApplication?> GetByIdAsync(Guid applicationId, Guid userId, CancellationToken ct = default);

  /// <summary>
  /// List applications for a user with filtering and pagination.
  /// </summary>
  Task<PaginatedResponse<JobApplication>> ListAsync(
      Guid userId,
      ApplicationStatus[]? statuses = null,
      string? company = null,
      ApplicationSource? source = null,
      DateTime? appliedFrom = null,
      DateTime? appliedTo = null,
      string? searchQuery = null,
      int pageSize = 20,
      string? continuationToken = null,
      CancellationToken ct = default);

  /// <summary>
  /// Update an application.
  /// </summary>
  Task<JobApplication> UpdateAsync(JobApplication application, CancellationToken ct = default);

  /// <summary>
  /// Delete an application.
  /// </summary>
  Task DeleteAsync(Guid applicationId, Guid userId, CancellationToken ct = default);

  /// <summary>
  /// Count applications for a user.
  /// </summary>
  Task<int> CountAsync(Guid userId, CancellationToken ct = default);
}
