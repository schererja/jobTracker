namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to register a new user account.
/// </summary>
public class RegisterRequest
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request to login with existing credentials.
/// </summary>
public class LoginRequest
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response containing authentication token and user info.
/// </summary>
public class AuthResponse
{
  public string Token { get; set; } = string.Empty;
  public string UserId { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
}
