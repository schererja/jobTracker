# Quick Reference - Local Testing Commands

## One-Command Startup (Recommended)
```bash
./start-local.sh
```
This script handles:
- ✅ Starting Cosmos DB Emulator (with Docker)
- ✅ Checking prerequisites
- ✅ Launching Azure Functions

## Manual Steps

### 1. Start Cosmos DB Emulator
```bash
# First time only (creates container):
docker run -d -p 8081:8081 \
  --name cosmos-emulator \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
  mcr.microsoft.com/cosmosdb/linux/cosmosdb

# Subsequent runs (just start existing container):
docker start cosmos-emulator

# Access Data Explorer:
# → https://localhost:8081/_explorer/index.html
```

### 2. Start Azure Functions
```bash
cd src/JobTracker.Api
func start
```

Functions will start on: **http://localhost:7071/api**

### 3. Test with curl
```bash
# Get user profile (creates user)
curl -X GET http://localhost:7071/api/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test"

# Create application
curl -X POST http://localhost:7071/api/applications \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test" \
  -H "Content-Type: application/json" \
  -d '{"company":"Google","roleTitle":"Engineer","location":"NYC","salaryRange":"150k-200k","appliedDate":"2025-12-11T00:00:00Z","status":"Applied","source":"LinkedIn","url":"https://example.com","resumeUsed":"resume.pdf","notes":"Great opportunity"}'

# List applications
curl -X GET http://localhost:7071/api/applications \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test"
```

## Test Token Breakdown
The Bearer token above is a JWT with claims:
```json
{
  "sub": "4f37d768-0e46-48f2-9344-8945d98e0222",
  "email": "test@example.com",
  "iat": 1700000000
}
```

**Replace `sub` with any GUID to test as different users.**

## Import Postman Collection
1. Open Postman
2. Click **Import** (top-left)
3. Select file: `jobTracker-api-postman.json`
4. Collection will appear with all endpoints pre-configured
5. Update token in **Auth** tab if needed

## Verify Everything Works

✅ **Cosmos DB running:**
```bash
curl -sk https://localhost:8081/_explorer/index.html | grep -q "Cosmos" && echo "✅ Ready" || echo "❌ Not ready"
```

✅ **Functions running:**
```bash
curl http://localhost:7071/api/me -H "Authorization: Bearer test" && echo "✅ Ready" || echo "❌ Not ready"
```

## Docker Troubleshooting

Stop Cosmos:
```bash
docker stop cosmos-emulator
```

Restart Cosmos:
```bash
docker start cosmos-emulator
```

Remove Cosmos (reset database):
```bash
docker stop cosmos-emulator
docker rm cosmos-emulator
# Re-run: docker run -d -p 8081:8081 ...
```

View Cosmos logs:
```bash
docker logs cosmos-emulator
```

## API Endpoints Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/me` | Get/create user profile |
| GET | `/api/applications` | List applications (with filters) |
| POST | `/api/applications` | Create application |
| GET | `/api/applications/{id}` | Get application |
| PATCH | `/api/applications/{id}` | Update application |
| DELETE | `/api/applications/{id}` | Delete application |
| POST | `/api/applications/{id}/status` | Update status |
| GET | `/api/applications/{id}/status-history` | Get status history |
| GET | `/api/applications/{id}/interviews` | List interviews |
| POST | `/api/applications/{id}/interviews` | Create interview |
| GET | `/api/applications/{id}/interviews/{eventId}` | Get interview |
| PATCH | `/api/applications/{id}/interviews/{eventId}` | Update interview |
| DELETE | `/api/applications/{id}/interviews/{eventId}` | Delete interview |
| POST | `/api/applications/{id}/attachments/presign-upload` | Get upload URL |
| POST | `/api/applications/{id}/attachments` | Confirm upload |
| GET | `/api/applications/{id}/attachments` | List attachments |
| GET | `/api/applications/{id}/attachments/{attachmentId}/presign-download` | Get download URL |
| DELETE | `/api/applications/{id}/attachments/{attachmentId}` | Delete attachment |

---

**Need help?** See [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md) for detailed setup instructions.
