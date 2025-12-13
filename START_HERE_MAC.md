# 🚀 Start Here - Apple Silicon Users

## Issue: Cosmos DB Emulator + Apple Silicon = 💥

**The Cosmos DB Linux emulator doesn't work reliably on M1/M2/M3 Macs.**

## ✅ Solution: Use Azure Cosmos DB Free Tier (5 minutes)

### Step 1: Create Free Account
👉 **https://aka.ms/cosmos-free-tier**

- No credit card required
- 1000 RU/s + 25GB free forever
- Create account named: `jobtracker-yourname`
- Enable "Free Tier Discount"

### Step 2: Get Connection String
After creation:
1. Go to your Cosmos account
2. Click **"Keys"** → Copy **PRIMARY CONNECTION STRING**

### Step 3: Update Config
Edit `src/JobTracker.Api/local.settings.json`:

```json
"CosmosDbConnectionString": "PASTE_YOUR_CONNECTION_STRING_HERE"
```

### Step 4: Start Functions
```bash
cd src/JobTracker.Api
func start
```

### Step 5: Test
Open Postman → Import `jobTracker-api-postman.json` → Start testing!

---

## 📋 What Got Fixed

✅ Docker credential helper issue → **FIXED**
⚠️ Cosmos emulator on ARM64 → **Use cloud free tier instead**

## 📚 Full Documentation

- [APPLE_SILICON_SETUP.md](APPLE_SILICON_SETUP.md) - Detailed troubleshooting
- [LOCAL_TESTING_GUIDE.md](LOCAL_TESTING_GUIDE.md) - Complete testing guide
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - Command reference

---

**Quick Link:** https://aka.ms/cosmos-free-tier

**Total time:** 5 minutes to get fully running ⚡
