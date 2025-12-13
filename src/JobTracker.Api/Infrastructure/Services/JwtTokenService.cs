using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Api.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
  private readonly IConfiguration _configuration;
  private readonly JwtSecurityTokenHandler _tokenHandler;

  public JwtTokenService(IConfiguration configuration)
  {
    _configuration = configuration;
    _tokenHandler = new JwtSecurityTokenHandler();
  }

  public string GenerateToken(Guid userId, string email)
  {
    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT secret key not configured"));

    var claims = new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")),
      Issuer = _configuration["Jwt:Issuer"],
      Audience = _configuration["Jwt:Audience"],
      SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = _tokenHandler.CreateToken(tokenDescriptor);
    return _tokenHandler.WriteToken(token);
  }

  public ClaimsPrincipal? ValidateToken(string token)
  {
    try
    {
      var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]
          ?? throw new InvalidOperationException("JWT secret key not configured"));

      var validationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = _configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = _configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };

      var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
      return principal;
    }
    catch
    {
      return null;
    }
  }
}
