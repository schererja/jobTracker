using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;

namespace JobTracker.Api.Functions;

public class UserFunctions
{
  private readonly IUserRepository _userRepo;
  private readonly IIdentityService _identity;
  private readonly ILogger<UserFunctions> _logger;

  public UserFunctions(IUserRepository userRepo, IIdentityService identity, ILogger<UserFunctions> logger)
  {
    _userRepo = userRepo;
    _identity = identity;
    _logger = logger;
  }

  [Function("GetProfile")]
  public async Task<HttpResponseData> GetProfile(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequestData req,
      CancellationToken ct)
  {
    try
    {
      _logger.LogInformation("GetProfile called");
      var userId = _identity.GetUserId(req);
      _logger.LogInformation("UserId: {UserId}", userId);
      var email = _identity.GetUserEmail(req);
      _logger.LogInformation("Email: {Email}", email);

      var user = await _userRepo.GetByIdAsync(userId, ct);
      if (user == null)
      {
        _logger.LogInformation("User not found, creating new user");
        // Create user if doesn't exist
        user = await _userRepo.GetOrCreateAsync(email, ct);
      }

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(user, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in GetProfile: {Message}", ex.Message);
      return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to get profile", ex.Message);
    }
  }

  private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    await response.WriteAsJsonAsync(new { error = message, details });
    return response;
  }
}
