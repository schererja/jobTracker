# 🍎 Apple Silicon Setup Issue - RESOLVED

## Problem Detected
You encountered two issues:
1. ✅ **FIXED:** Docker credential helper not in PATH
2. ⚠️ **KNOWN ISSUE:** Cosmos DB Linux emulator is unstable on Apple Silicon (M1/M2/M3)

## Solutions Applied

### 1. Docker Credential Fix ✅
Fixed the `docker-credential-osxkeychain` error by updating `~/.docker/config.json`.
- Backup saved to: `~/.docker/config.json.backup`
- Docker now works correctly

### 2. Cosmos DB on Apple Silicon

The Azure Cosmos DB Linux emulator **is not officially supported on ARM64 architecture** and frequently crashes.

## 🎯 RECOMMENDED: Use Azure Cosmos DB Free Tier

**Best option for Apple Silicon users:**

### Quick Setup (5 minutes)
1. Go to: **https://aka.ms/cosmos-free-tier**
2. Sign in (or create free Microsoft account)
3. Create Azure Cosmos DB account:
   - **Resource Group:** `jobtracker-dev`
   - **Account Name:** `jobtracker-yourname` (must be globally unique)
   - **Location:** Choose closest region
   - **Apply Free Tier Discount:** ✅ **YES**
   - **Capacity mode:** Serverless (or Provisioned with free tier)
4. Click **Review + create** → **Create** (takes ~2 minutes)

### Get Connection String
After deployment completes:
1. Go to your Cosmos DB account
2. Click **Keys** in left menu
3. Copy the **PRIMARY CONNECTION STRING**

### Update Local Settings
Edit `src/JobTracker.Api/local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "FUNCTIONS_WORKER_RUNTIME_VERSION": "8.0",
    "CosmosDbConnectionString": "AccountEndpoint=https://jobtracker-yourname.documents.azure.com:443/;AccountKey=YOUR_KEY_HERE==;",
    "CosmosDbDatabaseName": "jobtracker",
    "CosmosDbContainerName": "items"
  }
}
```

### Benefits of Free Tier
- ✅ **Always free** - 1000 RU/s + 25GB forever
- ✅ **No credit card** required
- ✅ **Full features** - same as paid tier
- ✅ **Better performance** than emulator
- ✅ **Reliable** - no crashes
- ✅ **Cloud testing** - identical to production
- ✅ **Data persistence** across machine restarts

## Alternative: Try Emulator (May Crash) ⚠️

If you really want to try the emulator locally:

```bash
./setup-cosmos-mac.sh
```

Choose **Option 2** - but be warned it may crash frequently.

## Once Cosmos DB is Ready

### Start Azure Functions
```bash
cd src/JobTracker.Api
func start
```

### Test API
```bash
curl -X GET http://localhost:7071/api/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZjM3ZDc2OC0wZTQ2LTQ4ZjItOTM0NC04OTQ1ZDk4ZTAyMjIiLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJpYXQiOjE3MDAwMDAwMDB9.test"
```

Or use Postman: Import `jobTracker-api-postman.json`

## Files Created

- ✅ `fix-docker-credentials.sh` - Fixed Docker config
- ✅ `setup-cosmos-mac.sh` - Interactive Cosmos setup for Mac
- ✅ Updated `start-local.sh` - Now includes Apple Silicon detection

## Next Steps

1. ✅ Docker credentials fixed
2. 🔄 **Set up Azure Cosmos DB Free Tier** (5 min) ← **DO THIS NOW**
3. 📝 Update `local.settings.json` with connection string
4. 🚀 Run `cd src/JobTracker.Api && func start`
5. 🧪 Test with Postman

---

**Quick Link:** https://aka.ms/cosmos-free-tier

**Need help?** See [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md) for full setup.
