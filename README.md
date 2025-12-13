# JobTracker

A serverless job application tracking system with a Blazor WebAssembly frontend and Azure Functions backend. Track your applications, interviews, status changes, and attachments all in one place.

## Architecture

- **Frontend:** Blazor WebAssembly (static hosting)
- **Backend:** Azure Functions (.NET 8 isolated worker)
- **Database:** Azure Cosmos DB (serverless, SQL API)
- **Storage:** Azure Blob Storage (presigned SAS for uploads/downloads)
- **Authentication:** Azure Static Web Apps built-in auth + Azure AD B2C (optional)

## Project Structure

```
.
├── JobTracker.sln                 # Solution file
├── api/
│   └── openapi.yaml              # REST API specification (OpenAPI 3.0.3)
├── src/
│   ├── JobTracker.Client/         # Blazor WASM frontend
│   ├── JobTracker.Api/            # Azure Functions backend
│   └── JobTracker.Shared/         # Shared DTOs, models, enums
├── .gitignore
└── README.md
```

## Quick Start

### Prerequisites

- .NET 8 SDK or higher
- Azure Functions Core Tools (for local development)
- Node.js 18+ (for Blazor WASM bundling)

### Local Development

**Build the solution:**
```bash
cd /Users/schererja/src/github.com/schererja/jobTracker
dotnet build
```

**Run Azure Functions API (terminal 1):**
```bash
cd src/JobTracker.Api
func start
```
The API will be available at `http://localhost:7071/api`

**Run Blazor WASM Client (terminal 2):**
```bash
cd src/JobTracker.Client
dotnet watch run
```
The client will be available at `http://localhost:5000`

## API Endpoints

All endpoints require JWT bearer token authentication. See [api/openapi.yaml](./api/openapi.yaml) for full specification.

### Applications
- `GET /api/applications` - List with filtering and pagination
- `POST /api/applications` - Create new
- `GET /api/applications/{id}` - Get details
- `PATCH /api/applications/{id}` - Update
- `DELETE /api/applications/{id}` - Delete

### Status History
- `POST /api/applications/{id}/status` - Update status (auto-creates history)
- `GET /api/applications/{id}/status-history` - Get history with pagination

### Interviews
- `GET /api/applications/{id}/interviews` - List interviews
- `POST /api/applications/{id}/interviews` - Create
- `PATCH /api/applications/{id}/interviews/{eventId}` - Update
- `DELETE /api/applications/{id}/interviews/{eventId}` - Delete

### Attachments
- `POST /api/applications/{id}/attachments/presign-upload` - Get presigned URL
- `POST /api/applications/{id}/attachments` - Confirm upload
- `GET /api/applications/{id}/attachments` - List
- `GET /api/applications/{id}/attachments/{attachmentId}/presign-download` - Download
- `DELETE /api/applications/{id}/attachments/{attachmentId}` - Delete

### User
- `GET /api/me` - Get current user profile

## Domain Model

### User
- UserId (GUID)
- Email
- CreatedAt
- Plan (free/premium)

### JobApplication
- ApplicationId (GUID)
- UserId (foreign key)
- Company, RoleTitle, Location, SalaryRange
- AppliedDate, Status, Source, URL
- ResumeUsed (optional), Notes
- CreatedAt, UpdatedAt

### InterviewEvent
- InterviewId (GUID)
- ApplicationId (foreign key)
- Date, Interviewer, Type (enum), Notes
- CreatedAt, UpdatedAt

### StatusHistory (Audit)
- HistoryId (GUID)
- ApplicationId (foreign key)
- OldStatus, NewStatus, ChangedAt

### Attachment
- AttachmentId (GUID)
- ApplicationId (foreign key)
- Type, FileName, ContentType, SizeBytes
- StoragePath, UploadedAt

## Cloud-First Design

This project is designed with cloud portability in mind:
- **Shared domain model** in `JobTracker.Shared` (no cloud SDK dependencies)
- **OpenAPI contract** (`api/openapi.yaml`) as the source of truth
- **Repository abstractions** for easy Azure → AWS migration
- **Opaque continuation tokens** and cloud-agnostic API responses

## Future: AWS Port

When ready, implement AWS adapters:
- API Gateway + Lambda (HTTP API)
- DynamoDB (single-table design)
- S3 (presigned URLs)
- Amazon Cognito (auth)
- CloudFront (WASM distribution)

All client-side code remains unchanged; only backend adapters swap.

## Configuration

### Local (Development)
- API: `http://localhost:7071/api`
- Auth: Azure Static Web Apps / SWA built-in (or local dev server)

### Azure (Production)
- API: `https://jobtracker.azurestaticapps.net/api`
- Auth: Azure AD B2C (via SWA or custom MSAL flow)

## Next Steps

1. Implement API endpoints in `src/JobTracker.Api`
2. Add Cosmos DB repository in `src/JobTracker.Api/Infrastructure`
3. Implement Blazor components in `src/JobTracker.Client/Pages`
4. Wire up authentication (SWA built-in or MSAL)
5. Deploy to Azure Static Web Apps + Functions

---

**Built with .NET 8, Blazor, Azure Functions, and Azure Cosmos DB**
