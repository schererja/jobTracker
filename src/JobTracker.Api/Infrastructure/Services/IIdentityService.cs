using Microsoft.Azure.Functions.Worker.Http;

namespace JobTracker.Api.Infrastructure.Services;

/// <summary>
/// Service to extract identity information from HTTP request context.
/// </summary>
public interface IIdentityService
{
  /// <summary>
  /// Get the current user ID from the JWT token.
  /// </summary>
  Guid GetUserId(HttpRequestData request);

  /// <summary>
  /// Get the current user's email from the JWT token.
  /// </summary>
  string GetUserEmail(HttpRequestData request);

  /// <summary>
  /// Get a claim value by type.
  /// </summary>
  string? GetClaim(string claimType);
}
