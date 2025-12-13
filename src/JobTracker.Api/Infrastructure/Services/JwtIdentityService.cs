using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;

namespace JobTracker.Api.Infrastructure.Services;

public class JwtIdentityService : IIdentityService
{
  private readonly IJwtTokenService _jwtTokenService;

  public JwtIdentityService(IJwtTokenService jwtTokenService)
  {
    _jwtTokenService = jwtTokenService;
  }

  public Guid GetUserId(HttpRequestData request)
  {
    var principal = GetPrincipal(request);
    var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? principal?.FindFirst("sub")?.Value;

    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
      throw new UnauthorizedAccessException("Invalid or missing user ID in token");
    }

    return userId;
  }

  public string GetUserEmail(HttpRequestData request)
  {
    var principal = GetPrincipal(request);
    var email = principal?.FindFirst(ClaimTypes.Email)?.Value
        ?? principal?.FindFirst("email")?.Value;

    if (string.IsNullOrEmpty(email))
    {
      throw new UnauthorizedAccessException("Invalid or missing email in token");
    }

    return email;
  }

  private ClaimsPrincipal? GetPrincipal(HttpRequestData request)
  {
    if (!request.Headers.TryGetValues("Authorization", out var authHeaders))
    {
      throw new UnauthorizedAccessException("Missing Authorization header");
    }

    var authHeader = authHeaders.FirstOrDefault();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
      throw new UnauthorizedAccessException("Invalid Authorization header format");
    }

    var token = authHeader.Substring("Bearer ".Length).Trim();
    var principal = _jwtTokenService.ValidateToken(token);

    if (principal == null)
    {
      throw new UnauthorizedAccessException("Invalid or expired token");
    }

    return principal;
  }

  public string? GetClaim(string claimType)
  {
    // Note: This method is legacy and not recommended for JWT-based auth
    // It requires storing the principal in a thread-local or context variable
    throw new NotImplementedException("GetClaim without request parameter is not supported in JWT auth. Use GetUserId or GetUserEmail instead.");
  }
}
