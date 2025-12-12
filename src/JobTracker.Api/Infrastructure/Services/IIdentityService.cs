using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace JobTracker.Api.Infrastructure.Services;

/// <summary>
/// Service to extract identity information from HTTP request context.
/// </summary>
public interface IIdentityService
{
  /// <summary>
  /// Get the current user ID from the JWT token.
  /// </summary>
  Guid GetUserId();

  /// <summary>
  /// Get the current user's email from the JWT token.
  /// </summary>
  string GetUserEmail();

  /// <summary>
  /// Get a claim value by type.
  /// </summary>
  string? GetClaim(string claimType);
}

/// <summary>
/// Default implementation using HttpContext.User.
/// </summary>
public class HttpContextIdentityService : IIdentityService
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public HttpContextIdentityService(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public Guid GetUserId()
  {
    var userIdClaim = GetClaim(ClaimTypes.NameIdentifier)
        ?? GetClaim("sub")
        ?? GetClaim("oid");

    if (string.IsNullOrEmpty(userIdClaim))
      throw new InvalidOperationException("User ID claim not found in token");

    if (Guid.TryParse(userIdClaim, out var userId))
      return userId;

    throw new InvalidOperationException($"Invalid user ID format: {userIdClaim}");
  }

  public string GetUserEmail()
  {
    var email = GetClaim(ClaimTypes.Email) ?? GetClaim("email");

    if (string.IsNullOrEmpty(email))
      throw new InvalidOperationException("Email claim not found in token");

    return email;
  }

  public string? GetClaim(string claimType)
  {
    var user = _httpContextAccessor.HttpContext?.User;
    if (user == null)
      return null;

    var claim = user.FindFirst(claimType);
    return claim?.Value;
  }
}
