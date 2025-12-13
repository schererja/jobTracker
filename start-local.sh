#!/bin/bash

# jobTracker Local Development Launcher
# Run this script to start all services needed for local development

set -e  # Exit on error

echo "🚀 Starting jobTracker Local Environment..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop."
    exit 1
fi

# Check if Cosmos DB Emulator is running
if ! docker ps | grep -q cosmos-emulator; then
    echo "📦 Cosmos DB Emulator not running. Starting..."
    docker start cosmos-emulator 2>/dev/null || {
        echo "🔧 Creating and starting Cosmos DB Emulator (first run)..."
        echo "   Note: Using --platform linux/amd64 for Apple Silicon compatibility"
        docker run -d -p 8081:8081 \
            --platform linux/amd64 \
            --name cosmos-emulator \
            -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 \
            -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
            mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
        
        echo "⏳ Waiting for Cosmos DB to be ready (this may take 1-2 minutes on first run)..."
        sleep 30
    }
    
    # Wait for Cosmos to be accessible
    for i in {1..30}; do
        if curl -sk https://localhost:8081/_explorer/index.html > /dev/null 2>&1; then
            echo "✅ Cosmos DB Emulator is ready"
            break
        fi
        echo "   Waiting... ($i/30)"
        sleep 2
    done
else
    echo "✅ Cosmos DB Emulator is running"
fi

# Check if Azure Functions Core Tools is installed
if ! command -v func &> /dev/null; then
    echo "❌ Azure Functions Core Tools not installed. Run: brew tap azure/tap && brew install azure-functions-core-tools@4"
    exit 1
fi

echo ""
echo "✅ All prerequisites are ready!"
echo ""
echo "🔗 Cosmos DB Explorer: https://localhost:8081/_explorer/index.html"
echo "🔗 API Base URL: http://localhost:7071/api"
echo ""
echo "Starting Azure Functions..."
echo "Press Ctrl+C to stop."
echo ""

cd "$(dirname "$0")/src/JobTracker.Api"
func start --build
