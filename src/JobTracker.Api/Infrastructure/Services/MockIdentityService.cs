using Microsoft.Azure.Functions.Worker.Http;

namespace JobTracker.Api.Infrastructure.Services;

/// <summary>
/// Mock identity service for local testing.
/// Returns hardcoded user ID and email that match the Postman collection.
/// </summary>
public class MockIdentityService : IIdentityService
{
  // This matches the userId in the Postman collection
  private const string MockUserId = "4f37d768-0e46-48f2-9344-8945d98e0222";
  private const string MockEmail = "test@example.com";

  public Guid GetUserId(HttpRequestData request)
  {
    return Guid.Parse(MockUserId);
  }

  public string GetUserEmail(HttpRequestData request)
  {
    return MockEmail;
  }

  public string? GetClaim(string claimType)
  {
    return claimType switch
    {
      "sub" => MockUserId,
      "email" => MockEmail,
      _ => null
    };
  }
}
