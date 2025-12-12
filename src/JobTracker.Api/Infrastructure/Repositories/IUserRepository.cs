using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Repository interface for user operations.
/// </summary>
public interface IUserRepository
{
  /// <summary>
  /// Get or create a user by email.
  /// </summary>
  Task<User> GetOrCreateAsync(string email, CancellationToken ct = default);

  /// <summary>
  /// Get a user by ID.
  /// </summary>
  Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);

  /// <summary>
  /// Get a user by email.
  /// </summary>
  Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

  /// <summary>
  /// Update user profile.
  /// </summary>
  Task<User> UpdateAsync(User user, CancellationToken ct = default);
}
