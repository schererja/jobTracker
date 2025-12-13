# Authentication Guide

This guide explains how to use the authentication system in the Job Tracker API.

## Overview

The API now includes a complete JWT-based authentication system with user registration and login. Users can create accounts, authenticate, and manage their own job applications.

## Environment Modes

The API supports two modes:

### Development Mode (Local Testing)
- Uses `MockIdentityService` - no authentication required
- All requests use a hardcoded test user (ID: `4f37d768-0e46-48f2-9344-8945d98e0222`)
- No Authorization header needed
- Good for quick testing and development

### Production Mode
- Uses `JwtIdentityService` - authentication required
- All protected endpoints require a valid JWT token in the Authorization header
- Users must register/login to get a token
- Each user only sees their own data

## API Endpoints

### Register a New User

Create a new user account and receive a JWT token.

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "your-secure-password"
}
```

**Response (201 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "4f37d768-0e46-48f2-9344-8945d98e0222",
  "email": "user@example.com"
}
```

**Curl Example:**
```bash
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'
```

### Login

Authenticate an existing user and receive a JWT token.

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "your-secure-password"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "4f37d768-0e46-48f2-9344-8945d98e0222",
  "email": "user@example.com"
}
```

**Curl Example:**
```bash
curl -X POST http://localhost:7071/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'
```

## Using JWT Tokens

After registering or logging in, save the `token` value from the response. Include it in the `Authorization` header for all protected endpoints:

```bash
# Get your profile
curl -X GET http://localhost:7071/api/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Create an application
curl -X POST http://localhost:7071/api/applications \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Acme Corp",
    "position": "Software Engineer",
    "applicationDate": "2024-01-15"
  }'

# List your applications
curl -X GET http://localhost:7071/api/applications \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Testing Multi-User Scenarios

To test that users can only see their own data:

### 1. Register User A
```bash
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"pass123"}'
```

Save the token as `TOKEN_A`.

### 2. Register User B
```bash
curl -X POST http://localhost:7071/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"bob@example.com","password":"pass456"}'
```

Save the token as `TOKEN_B`.

### 3. Create Application for User A
```bash
curl -X POST http://localhost:7071/api/applications \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Company A",
    "position": "Developer",
    "applicationDate": "2024-01-15"
  }'
```

### 4. Create Application for User B
```bash
curl -X POST http://localhost:7071/api/applications \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Company B",
    "position": "Engineer",
    "applicationDate": "2024-01-16"
  }'
```

### 5. Verify Isolation
```bash
# User A should only see Company A
curl -X GET http://localhost:7071/api/applications \
  -H "Authorization: Bearer $TOKEN_A"

# User B should only see Company B
curl -X GET http://localhost:7071/api/applications \
  -H "Authorization: Bearer $TOKEN_B"
```

## Configuration

JWT settings are configured in `local.settings.json`:

```json
{
  "Values": {
    "Jwt:SecretKey": "your-super-secret-jwt-key-that-must-be-at-least-32-characters-long-for-production",
    "Jwt:Issuer": "jobtracker-api",
    "Jwt:Audience": "jobtracker-client",
    "Jwt:ExpirationHours": "24"
  }
}
```

**Important:** Change the `Jwt:SecretKey` to a secure random value in production!

## Error Responses

### 400 Bad Request
Missing or invalid request body:
```json
{
  "error": "Email and password are required"
}
```

### 401 Unauthorized
Invalid credentials or token:
```json
{
  "error": "Invalid email or password"
}
```

Or for expired/invalid tokens:
```json
{
  "error": "Invalid or expired token"
}
```

### 409 Conflict
Email already registered:
```json
{
  "error": "User with this email already exists"
}
```

## Security Notes

- Passwords are hashed using BCrypt before storage
- JWT tokens are signed with HS256 algorithm
- Tokens expire after 24 hours by default (configurable)
- Never share your JWT secret key
- Use HTTPS in production to protect tokens in transit

## Switching Between Modes

The API automatically detects the environment:

- **Development:** Set `ASPNETCORE_ENVIRONMENT=Development` or `DOTNET_ENVIRONMENT=Development`
- **Production:** Set `ASPNETCORE_ENVIRONMENT=Production` or `DOTNET_ENVIRONMENT=Production`

To test production mode locally:
```bash
export DOTNET_ENVIRONMENT=Production
func start
```

To return to development mode:
```bash
export DOTNET_ENVIRONMENT=Development
func start
```

## Postman Setup

1. **Register a user:**
   - Create a POST request to `http://localhost:7071/api/auth/register`
   - Set body to raw JSON with email and password
   - Send request and save the token

2. **Save token as environment variable:**
   - In Postman, create an environment variable called `authToken`
   - Set its value to the token from the register/login response

3. **Use token in requests:**
   - For protected endpoints, add a header: `Authorization: Bearer {{authToken}}`
   - Or use Postman's Authorization tab and select "Bearer Token"

## Troubleshooting

### "Missing Authorization header"
- Make sure you include the `Authorization` header
- Format must be: `Authorization: Bearer <token>`

### "Invalid or expired token"
- Token may have expired (default 24 hours)
- Login again to get a new token
- Check that you copied the full token

### "User with this email already exists"
- Use a different email or login with the existing account
- You can check Cosmos DB to see all registered users
