using System.Text.Json.Serialization;

namespace JobTracker.Shared.Models;

/// <summary>
/// Represents a user of the job tracking application.
/// </summary>
public class User
{
  /// <summary>
  /// Unique identifier for the user (required by Cosmos DB).
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// User ID for partition key (required by Cosmos DB).
  /// </summary>
  public string UserId { get; set; } = string.Empty;

  /// <summary>
  /// User's email address (used for authentication).
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Timestamp when the user account was created.
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// User's subscription plan (e.g., "free", "premium").
  /// </summary>
  public string Plan { get; set; } = "free";

  /// <summary>
  /// Hashed password for authentication (BCrypt hash).
  /// </summary>
  public string? PasswordHash { get; set; }
}
