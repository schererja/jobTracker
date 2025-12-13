#!/bin/bash

# Quick Azure Cosmos DB Free Tier Setup
# Run this script once, then update local.settings.json with your connection string

echo "🌐 Azure Cosmos DB Free Tier Setup"
echo ""
echo "Follow these steps:"
echo ""
echo "1️⃣  Open: https://aka.ms/cosmos-free-tier"
echo ""
echo "2️⃣  Sign in (or create free Microsoft account - NO CREDIT CARD)"
echo ""
echo "3️⃣  Click 'Create' → 'Azure Cosmos DB for NoSQL'"
echo ""
echo "4️⃣  Fill in:"
echo "   Resource Group: jobtracker-dev"
echo "   Account Name: jobtracker-yourname (must be unique)"
echo "   Location: pick closest to you"
echo "   ✅ Apply Free Tier Discount: YES"
echo ""
echo "5️⃣  Click 'Review + create' → 'Create' (takes ~2 minutes)"
echo ""
echo "6️⃣  After deployment:"
echo "   - Go to your Cosmos account"
echo "   - Click 'Keys' in left menu"
echo "   - Copy the 'PRIMARY CONNECTION STRING'"
echo ""
echo "7️⃣  Paste your connection string below:"
echo ""
read -p "Enter your PRIMARY CONNECTION STRING: " CONNECTION_STRING

if [ -z "$CONNECTION_STRING" ]; then
    echo "❌ No connection string provided"
    exit 1
fi

# Update local.settings.json
CONFIG_FILE="src/JobTracker.Api/local.settings.json"

if [ ! -f "$CONFIG_FILE" ]; then
    echo "❌ Could not find $CONFIG_FILE"
    exit 1
fi

# Escape the connection string for sed
ESCAPED_CONNECTION_STRING=$(echo "$CONNECTION_STRING" | sed 's/[&/\]/\\&/g')

# Update the file
sed -i '' "s|\"CosmosDbConnectionString\": \"[^\"]*\"|\"CosmosDbConnectionString\": \"$ESCAPED_CONNECTION_STRING\"|" "$CONFIG_FILE"

echo ""
echo "✅ Updated $CONFIG_FILE with your Azure connection string"
echo ""
echo "Next steps:"
echo "1. Restart Azure Functions: func start"
echo "2. Test with Postman"
echo ""
echo "You now have:"
echo "  ✅ 1000 RU/s free throughput"
echo "  ✅ 25GB storage free"
echo "  ✅ Forever free (no expiration)"
echo "  ✅ Same features as paid tier"
