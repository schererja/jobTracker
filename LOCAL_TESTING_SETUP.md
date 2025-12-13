# Local Testing Setup - Complete ✅

Your jobTracker application is ready for local testing! Here's what's been set up:

## 📁 New Files Created

1. **[start-local.sh](start-local.sh)** - Automated startup script
   - Starts Cosmos DB Emulator (Docker)
   - Checks prerequisites
   - Launches Azure Functions
   - **Usage:** `./start-local.sh`

2. **[LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md)** - Comprehensive setup guide
   - Step-by-step installation instructions
   - Cosmos DB setup
   - Function startup
   - API endpoint testing with curl
   - Postman integration
   - Troubleshooting

3. **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick lookup guide
   - One-command startup
   - Manual startup steps
   - curl command examples
   - API endpoints table
   - Docker troubleshooting

4. **[jobTracker-api-postman.json](jobTracker-api-postman.json)** - Postman collection
   - Pre-configured for all endpoints
   - Test requests for all CRUD operations
   - Bearer token already set
   - Ready to import into Postman

## 🚀 Getting Started (3 Steps)

### Option A: Automated (Recommended)
```bash
./start-local.sh
```
Handles everything automatically.

### Option B: Manual
```bash
# Terminal 1: Start Cosmos DB
docker start cosmos-emulator  # or docker run if first time (see guide)

# Terminal 2: Start Functions
cd src/JobTracker.Api
func start
```

## ✅ Prerequisites Checklist

- [ ] Docker Desktop installed and running
- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] Azure Functions Core Tools v4 (`brew tap azure/tap && brew install azure-functions-core-tools@4`)
- [ ] Project builds successfully (`dotnet build` ✓ already done)

## 🧪 Testing Approaches

### 1. **Postman (Easiest)**
   - Import `jobTracker-api-postman.json` into Postman
   - Pre-configured endpoints with test data
   - No command line needed

### 2. **curl (Quick Manual Testing)**
   ```bash
   curl -X GET http://localhost:7071/api/me \
     -H "Authorization: Bearer <token>"
   ```

### 3. **Browser**
   - Cosmos DB Explorer: https://localhost:8081/_explorer/index.html
   - View database/container structure

### 4. **VS Code Terminal**
   - Run functions in debug mode
   - View live logs
   - Set breakpoints in function code

## 🔍 Key Components

| Component | Port | Status |
|-----------|------|--------|
| **Cosmos DB Emulator** | 8081 | Runs in Docker |
| **Azure Functions** | 7071 | Local dev server |
| **API Base URL** | http://localhost:7071/api | All endpoints |

## 📊 Test Workflow Example

1. **Get/Create User** → `GET /api/me`
2. **Create Application** → `POST /api/applications`
3. **List Applications** → `GET /api/applications`
4. **Get Single App** → `GET /api/applications/{id}`
5. **Create Interview** → `POST /api/applications/{id}/interviews`
6. **Update Status** → `POST /api/applications/{id}/status`
7. **Get Status History** → `GET /api/applications/{id}/status-history`
8. **Upload Attachment** → `POST /api/applications/{id}/attachments/presign-upload`

## 🔐 Authentication

Uses bearer token (JWT):
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test
```

**To test as different users:** Replace `sub` claim (GUID) in token.

## 📝 Project Structure

```
jobTracker/
├── src/
│   ├── JobTracker.Client/        # Blazor WASM frontend (not started yet)
│   ├── JobTracker.Api/            # Azure Functions backend ✅ Ready
│   │   ├── Functions/             # HTTP Triggers
│   │   ├── Infrastructure/
│   │   │   ├── Repositories/      # Cosmos DB implementations
│   │   │   └── Services/          # Identity, Storage
│   │   ├── local.settings.json    # Dev config (Cosmos, storage)
│   │   └── Program.cs             # DI setup
│   └── JobTracker.Shared/         # Domain models, DTOs ✅
├── api/
│   └── openapi.yaml               # API specification
├── LOCAL_TESTING_GUIDE.md         # 📄 Detailed setup (this folder)
├── QUICK_REFERENCE.md             # 📄 Quick lookup
├── start-local.sh                 # 🚀 Automated launcher
└── jobTracker-api-postman.json    # 📮 Postman collection
```

## 🐛 Common Issues & Fixes

| Issue | Solution |
|-------|----------|
| "Docker is not running" | Start Docker Desktop |
| Cosmos port already in use | `docker rm cosmos-emulator` |
| Functions can't find Cosmos | Check `local.settings.json` connection string |
| 401 Unauthorized | Verify Authorization header with Bearer token |
| "Cannot connect to cosmos:8081" | Wait 1-2 min for emulator startup |

## ✨ Next Steps After Testing

1. **Build Blazor Client**
   - Create pages to consume API endpoints
   - Integrate authentication (Azure AD B2C or SWA)
   - Upload component with presigned URLs

2. **Deploy to Azure**
   - Static Web Apps for Blazor client
   - Azure Functions for API
   - Cosmos DB (serverless, shared throughput)
   - Azure Blob Storage for attachments

3. **Add Production Auth**
   - Replace mock `HttpContextIdentityService` with real Azure AD
   - Enable CORS for WASM client
   - Rate limiting, API key management

4. **Implement Real Storage**
   - Replace `MockStorageService` with `AzureBlobStorageService`
   - Configure SAS token generation
   - Handle file lifecycle

---

**Ready to test?** Start with:
```bash
./start-local.sh
```

Then open Postman and import `jobTracker-api-postman.json`.

Happy testing! 🎉
