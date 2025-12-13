# Local Testing Setup Guide

This guide walks you through setting up and running the jobTracker application locally with Azure Functions and Cosmos DB.

## Prerequisites

- macOS with Homebrew installed
- Docker installed (for Cosmos DB emulator)
- .NET 8 SDK (should already be installed)
- Terminal/zsh

## 1. Install Azure Functions Core Tools

```bash
brew tap azure/tap
brew install azure-functions-core-tools@4
```

Verify installation:

```bash
func --version
```

## 2. Start Cosmos DB Emulator

### ⚠️ Apple Silicon (M1/M2/M3) Users

**The Cosmos DB Linux emulator is unstable on Apple Silicon and frequently crashes.**

**RECOMMENDED:** Use Azure Cosmos DB Free Tier instead:

- 👉 **Quick Setup:** [START_HERE_MAC.md](START_HERE_MAC.md)
- ✅ Free forever (1000 RU/s + 25GB)
- ✅ No crashes, better performance
- ⏱️ Takes only 5 minutes

### Intel Mac or Windows/Linux Users

The Cosmos DB emulator runs in Docker. Start it with:

```bash
docker run -d -p 8081:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 \
  --name cosmos-emulator \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
```

**For Apple Silicon (experimental, may crash):**

```bash
docker run -d -p 8081:8081 \
  --platform linux/amd64 \
  --name cosmos-emulator \
  -m 3g --cpus=2 \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
```

**First run:** Wait 1-2 minutes for startup to complete.

**Subsequent runs:**

```bash
docker start cosmos-emulator
```

### Verify Emulator is Running

Open in browser:

```
https://localhost:8081/_explorer/index.html
```

You should see the Cosmos Data Explorer (ignore SSL certificate warnings).

## 3. Initialize Cosmos DB Database & Container

The Functions will auto-create the database and container on first run, but you can pre-create them in Data Explorer:

1. Click **New Container** in Data Explorer
2. Database ID: `jobtracker`
3. Container ID: `items`
4. Partition key: `/userId`
5. Click **Create**

## 4. Start Azure Functions Locally

In the project root:

```bash
cd src/JobTracker.Api
func start
```

You should see output like:

```
Azure Functions Core Tools
Version: 4.x.x
...

Functions in JobTracker.Api:
    ApplicationFunctions.cs - [GET,POST] http://localhost:7071/api/applications
    ApplicationFunctions.cs - [GET,PATCH,DELETE] http://localhost:7071/api/applications/{id}
    StatusFunctions.cs - [POST] http://localhost:7071/api/applications/{id}/status
    StatusFunctions.cs - [GET] http://localhost:7071/api/applications/{id}/status-history
    InterviewFunctions.cs - [GET,POST] http://localhost:7071/api/applications/{id}/interviews
    InterviewFunctions.cs - [GET,PATCH,DELETE] http://localhost:7071/api/applications/{id}/interviews/{eventId}
    AttachmentFunctions.cs - [POST] http://localhost:7071/api/applications/presign-upload
    AttachmentFunctions.cs - [GET,POST] http://localhost:7071/api/applications/{id}/attachments
    AttachmentFunctions.cs - [GET] http://localhost:7071/api/applications/{id}/attachments/{attachmentId}/presign-download
    AttachmentFunctions.cs - [DELETE] http://localhost:7071/api/applications/{id}/attachments/{attachmentId}
    UserFunctions.cs - [GET] http://localhost:7071/api/me

Http Functions:
    http://localhost:7071/api/applications
    http://localhost:7071/api/me
```

**Keep this terminal running** while testing.

## 5. Test the API Endpoints

Open a new terminal. You'll need a valid JWT token for authorization. For local testing, use this test token (it contains userId and email claims):

### Create a Test User First

```bash
curl -X GET http://localhost:7071/api/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test" \
  -H "Content-Type: application/json"
```

Replace `4f37d768-0e46-48f2-9344-8945d98e0222` with your test user ID (any GUID).

### Create a Job Application

```bash
curl -X POST http://localhost:7071/api/applications \
  -H "Authorization: Bearer YOUR_TEST_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "company": "Acme Corp",
    "roleTitle": "Senior Software Engineer",
    "location": "San Francisco, CA",
    "salaryRange": "$150k - $200k",
    "appliedDate": "2025-12-11T00:00:00Z",
    "status": "Applied",
    "source": "LinkedIn",
    "url": "https://example.com/job/123",
    "resumeUsed": "resume-v2.pdf",
    "notes": "Great company, exciting product"
  }'
```

### List Applications

```bash
curl -X GET "http://localhost:7071/api/applications" \
  -H "Authorization: Bearer YOUR_TEST_TOKEN" \
  -H "Content-Type: application/json"
```

### Add an Interview

```bash
curl -X POST http://localhost:7071/api/applications/{applicationId}/interviews \
  -H "Authorization: Bearer YOUR_TEST_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2025-12-15T10:00:00Z",
    "interviewer": "John Smith",
    "type": "PhoneScreen",
    "notes": "Discuss experience with cloud architecture"
  }'
```

Replace `{applicationId}` with the ID from the create response.

### Update Application Status

```bash
curl -X POST http://localhost:7071/api/applications/{applicationId}/status \
  -H "Authorization: Bearer YOUR_TEST_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Interviewing"
  }'
```

### Get Status History

```bash
curl -X GET "http://localhost:7071/api/applications/{applicationId}/status-history" \
  -H "Authorization: Bearer YOUR_TEST_TOKEN"
```

### Upload Attachment (Presign)

```bash
curl -X POST http://localhost:7071/api/applications/{applicationId}/attachments/presign-upload \
  -H "Authorization: Bearer YOUR_TEST_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fileName": "cover-letter.pdf",
    "contentType": "application/pdf",
    "sizeBytes": 125000
  }'
```

This returns a presigned URL (currently mock).

## 6. Using Postman (Recommended)

1. Download [Postman](https://www.postman.com/downloads/)
2. Import the OpenAPI spec: [api/openapi.yaml](api/openapi.yaml)
3. Set up authorization:
   - Go to Collections → Auth
   - Type: Bearer Token
   - Token: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test`
4. Test endpoints directly in Postman UI

## 7. Monitor Function Logs

In the Functions terminal (step 4), you'll see live logs of all requests:

```
[11-Dec-2025 14:32:01] Executing 'ApplicationFunctions.CreateApplication' (Reason='This function was programmatically called via the host APIs.', Id=...)
[11-Dec-2025 14:32:02] Executed 'ApplicationFunctions.CreateApplication' (Succeeded, ...)
```

## 8. Troubleshooting

### Cosmos DB Emulator Won't Start

```bash
docker logs cosmos-emulator
```

If persistent issues:

```bash
docker rm cosmos-emulator
# Re-run the docker run command from step 2
```

### SSL Certificate Errors

Emulator uses self-signed certs. The functions ignore them, but browsers will warn. Safe to ignore.

### Functions Can't Connect to Cosmos

- Verify emulator is running: `docker ps | grep cosmos`
- Check connection string in `src/JobTracker.Api/local.settings.json`
- Default is: `AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPDAe8F7gIJlJ0/7mB+sjeIQg==;`

### 401 Unauthorized

- Ensure Bearer token is included in Authorization header
- Format: `Authorization: Bearer <token>`

## 9. Next Steps

1. **Test all CRUD operations** on each entity (application, interview, attachment, status history)
2. **Build Blazor Client** pages to consume these APIs
3. **Add proper authentication** (Azure AD B2C or SWA built-in auth)
4. **Deploy to Azure** with Static Web Apps + Azure Functions

---

**Happy testing!** 🚀
