using System.Security.Claims;

namespace JobTracker.Api.Infrastructure.Services;

public interface IJwtTokenService
{
  string GenerateToken(Guid userId, string email);
  ClaimsPrincipal? ValidateToken(string token);
}
