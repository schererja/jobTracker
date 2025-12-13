#!/bin/bash

# Fix Docker Credential Helper Issue
# This script fixes the "docker-credential-osxkeychain: executable file not found" error

echo "🔧 Fixing Docker credential helper..."

# Backup existing config
if [ -f ~/.docker/config.json ]; then
    cp ~/.docker/config.json ~/.docker/config.json.backup
    echo "✅ Backed up existing config to ~/.docker/config.json.backup"
fi

# Create new config without credsStore (simplest fix)
cat > ~/.docker/config.json << 'EOF'
{
  "auths": {},
  "currentContext": "desktop-linux"
}
EOF

echo "✅ Updated Docker config (removed credsStore requirement)"
echo ""
echo "Alternative: Add credential helper to PATH with:"
echo "   export PATH=\"/Applications/Docker.app/Contents/Resources/bin:\$PATH\""
echo ""
echo "Now try running: ./start-local.sh"
