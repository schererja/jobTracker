#!/bin/bash

# Apple Silicon Cosmos DB Setup
# The Linux Cosmos DB emulator has issues on Apple Silicon (ARM64/M1/M2/M3)

echo "🍎 Apple Silicon Detected"
echo ""
echo "⚠️  The Cosmos DB Linux emulator is unstable on Apple Silicon."
echo ""
echo "📋 You have 3 options:"
echo ""
echo "Option 1: Use Azure Cosmos DB Free Tier (RECOMMENDED) ✅"
echo "  • No credit card required"
echo "  • 1000 RU/s + 25GB storage free forever"
echo "  • Best performance and reliability"
echo "  • Setup: https://aka.ms/cosmos-free-tier"
echo ""
echo "Option 2: Use Cosmos DB Docker with Rosetta (EXPERIMENTAL) ⚠️"
echo "  • May crash or be unstable"
echo "  • Slower performance (x86 emulation)"
echo "  • Run: docker run --platform linux/amd64 -m 3g ..."
echo ""
echo "Option 3: Use Mock Repository (DEVELOPMENT ONLY) 🔧"
echo "  • In-memory data (lost on restart)"
echo "  • No Cosmos SDK testing"
echo "  • Fast iteration for UI development"
echo ""
read -p "Enter choice (1/2/3): " choice

case $choice in
    1)
        echo ""
        echo "🌐 Setting up Azure Cosmos DB Free Tier..."
        echo ""
        echo "Steps:"
        echo "1. Go to: https://aka.ms/cosmos-free-tier"
        echo "2. Sign in with your Microsoft account (or create one)"
        echo "3. Click 'Create Azure Cosmos DB account'"
        echo "4. Choose 'Azure Cosmos DB for NoSQL'"
        echo "5. Set:"
        echo "   - Resource Group: jobtracker-dev"
        echo "   - Account Name: jobtracker-[yourname]"
        echo "   - Location: (closest to you)"
        echo "   - Apply Free Tier Discount: Yes"
        echo "6. Click 'Review + create' → 'Create'"
        echo "7. After deployment, go to 'Keys' section"
        echo "8. Copy the PRIMARY CONNECTION STRING"
        echo ""
        echo "Then update src/JobTracker.Api/local.settings.json:"
        echo '   "CosmosDbConnectionString": "YOUR_CONNECTION_STRING_HERE"'
        echo ""
        ;;
    2)
        echo ""
        echo "⚠️  Attempting Cosmos DB emulator with Rosetta emulation..."
        echo ""
        docker rm -f cosmos-emulator 2>/dev/null
        docker run -d \
            --platform linux/amd64 \
            -p 8081:8081 \
            -p 10251:10251 \
            -p 10252:10252 \
            -p 10253:10253 \
            -p 10254:10254 \
            --name cosmos-emulator \
            -m 3g \
            --cpus=2 \
            -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
            -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=false \
            mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
        
        echo ""
        echo "⏳ Waiting for emulator to start (this may take 2-3 minutes)..."
        sleep 30
        
        if docker ps | grep -q cosmos-emulator; then
            echo "✅ Emulator appears to be running"
            echo "🔗 Data Explorer: https://localhost:8081/_explorer/index.html"
            echo ""
            echo "⚠️  If it crashes, use Option 1 (Free Tier) instead"
        else
            echo "❌ Emulator failed to start"
            echo "→ Recommend using Option 1 (Azure Free Tier)"
        fi
        ;;
    3)
        echo ""
        echo "🔧 Mock repository setup not yet implemented."
        echo "→ Use Option 1 (Azure Free Tier) for now"
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac
