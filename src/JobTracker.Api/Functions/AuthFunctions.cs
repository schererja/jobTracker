using System.Net;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;
using JobTracker.Shared.DTOs;
using JobTracker.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace JobTracker.Api.Functions;

public class AuthFunctions
{
  private readonly ILogger<AuthFunctions> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtTokenService _jwtTokenService;

  public AuthFunctions(
      ILogger<AuthFunctions> logger,
      IUserRepository userRepository,
      IJwtTokenService jwtTokenService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _jwtTokenService = jwtTokenService;
  }

  [Function("Register")]
  public async Task<HttpResponseData> Register(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
  {
    try
    {
      var request = await req.ReadFromJsonAsync<RegisterRequest>();
      if (request == null)
      {
        return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
      }

      // Validate input
      if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
      {
        return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email and password are required");
      }

      // Check if user already exists
      var existingUser = await _userRepository.GetByEmailAsync(request.Email);
      if (existingUser != null)
      {
        return await CreateErrorResponse(req, HttpStatusCode.Conflict, "User with this email already exists");
      }

      // Hash password
      var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

      // Create user
      var userId = Guid.NewGuid();
      var user = new User
      {
        Id = userId.ToString(),
        UserId = userId.ToString(),
        Email = request.Email,
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow,
        Plan = "free"
      };

      await _userRepository.CreateAsync(user);

      // Generate JWT token
      var token = _jwtTokenService.GenerateToken(userId, request.Email);

      var response = req.CreateResponse(HttpStatusCode.Created);
      await response.WriteAsJsonAsync(new AuthResponse
      {
        Token = token,
        UserId = userId.ToString(),
        Email = request.Email
      });

      _logger.LogInformation("User registered successfully: {Email}", request.Email);
      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error registering user");
      return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred during registration");
    }
  }

  [Function("Login")]
  public async Task<HttpResponseData> Login(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
  {
    try
    {
      var request = await req.ReadFromJsonAsync<LoginRequest>();
      if (request == null)
      {
        return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
      }

      // Validate input
      if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
      {
        return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email and password are required");
      }

      // Get user by email
      var user = await _userRepository.GetByEmailAsync(request.Email);
      if (user == null)
      {
        return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid email or password");
      }

      // Verify password
      if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
      {
        return await CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid email or password");
      }

      // Parse userId
      if (!Guid.TryParse(user.UserId, out var userId))
      {
        _logger.LogError("Invalid userId format for user {Email}", user.Email);
        return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
      }

      // Generate JWT token
      var token = _jwtTokenService.GenerateToken(userId, user.Email);

      var response = req.CreateResponse(HttpStatusCode.OK);
      await response.WriteAsJsonAsync(new AuthResponse
      {
        Token = token,
        UserId = user.UserId,
        Email = user.Email
      });

      _logger.LogInformation("User logged in successfully: {Email}", user.Email);
      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error logging in user");
      return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred during login");
    }
  }

  private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
  {
    var response = req.CreateResponse(statusCode);
    await response.WriteAsJsonAsync(new { error = message });
    return response;
  }
}
