using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;

namespace JobTracker.Api.Functions;

public class UserFunctions
{
  private readonly IUserRepository _userRepo;
  private readonly IIdentityService _identity;

  public UserFunctions(IUserRepository userRepo, IIdentityService identity)
  {
    _userRepo = userRepo;
    _identity = identity;
  }

  [Function("GetProfile")]
  public async Task<HttpResponseData> GetProfile(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequestData req,
      CancellationToken ct)
  {
    try
    {
      var userId = _identity.GetUserId();
      var email = _identity.GetUserEmail();

      var user = await _userRepo.GetByIdAsync(userId, ct);
      if (user == null)
      {
        // Create user if doesn't exist
        user = await _userRepo.GetOrCreateAsync(email, ct);
      }

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(user, cancellationToken: ct);
      return response;
    }
    catch (Exception ex)
    {
      return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Failed to get profile", ex.Message);
    }
  }

  private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode status, string message, string? details = null)
  {
    var response = req.CreateResponse(status);
    response.Headers.Add("Content-Type", "application/json");
    return response;
  }
}
