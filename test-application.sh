#!/bin/bash

# Job Tracker API Test Script
# This script registers/logs in a user and creates a test application

API_URL="http://localhost:7071"
EMAIL="testuser@example.com"
PASSWORD="SecurePass123"

echo "========================================"
echo "Job Tracker API - Application Test"
echo "========================================"
echo ""

# Try to login first
echo "1. Attempting to login..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

# Check if login was successful by looking for Token field
if echo "$LOGIN_RESPONSE" | grep -q "Token"; then
    echo "✓ Login successful!"
    TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"Token":"[^"]*"' | cut -d'"' -f4)
    USER_ID=$(echo "$LOGIN_RESPONSE" | grep -o '"UserId":"[^"]*"' | cut -d'"' -f4)
else
    # If login failed, try to register
    echo "✗ Login failed, attempting to register new user..."
    REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/register" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
    
    if echo "$REGISTER_RESPONSE" | grep -q "Token"; then
        echo "✓ Registration successful!"
        TOKEN=$(echo "$REGISTER_RESPONSE" | grep -o '"Token":"[^"]*"' | cut -d'"' -f4)
        USER_ID=$(echo "$REGISTER_RESPONSE" | grep -o '"UserId":"[^"]*"' | cut -d'"' -f4)
    else
        echo "✗ Registration failed!"
        echo "Response: $REGISTER_RESPONSE"
        exit 1
    fi
fi

echo ""
echo "User ID: $USER_ID"
echo "Token: ${TOKEN:0:50}..."
echo ""

# Create 5 applications with random data
echo "2. Creating 5 test applications..."

COMPANIES=("Acme Corp" "TechStart Inc" "Global Systems" "Innovation Labs" "DataFlow Solutions")
ROLES=("Senior Software Engineer" "Full Stack Developer" "Backend Engineer" "DevOps Engineer" "Platform Engineer")
LOCATIONS=("San Francisco, CA" "New York, NY" "Austin, TX" "Seattle, WA" "Remote")
NOTES=("Applied through company website" "Referred by colleague" "Found on LinkedIn" "Applied via recruiter" "Direct application")

for i in {1..5}; do
    # Random status (0-5: Applied, Interviewing, Offer, Rejected, Ghosted, Withdrawn)
    STATUS=$((RANDOM % 6))
    # Random source (0-6: LinkedIn, Indeed, Glassdoor, CompanyWebsite, Referral, Recruiter, Other)
    SOURCE=$((RANDOM % 7))
    # Random company, role, location, notes
    COMPANY="${COMPANIES[$((RANDOM % 5))]}"
    ROLE="${ROLES[$((RANDOM % 5))]}"
    LOCATION="${LOCATIONS[$((RANDOM % 5))]}"
    NOTE="${NOTES[$((RANDOM % 5))]}"
    
    echo "  Creating application $i: $COMPANY - $ROLE"
    
    APP_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST "$API_URL/api/applications" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"company\": \"$COMPANY\",
        \"roleTitle\": \"$ROLE\",
        \"location\": \"$LOCATION\",
        \"appliedDate\": \"2024-12-12T00:00:00Z\",
        \"status\": $STATUS,
        \"source\": $SOURCE,
        \"notes\": \"$NOTE\"
      }")
    
    HTTP_CODE=$(echo "$APP_RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
    
    if [ "$HTTP_CODE" = "201" ] || [ "$HTTP_CODE" = "200" ]; then
        echo "  ✓ Application $i created"
    else
        echo "  ✗ Application $i failed (HTTP $HTTP_CODE)"
    fi
done

echo ""
echo "Waiting for Cosmos DB to sync..."
sleep 3
echo ""

# List all applications for this user
echo "3. Listing all applications for user..."
LIST_RESPONSE=$(curl -s -X GET "$API_URL/api/applications" \
  -H "Authorization: Bearer $TOKEN")

# Pretty print the response
echo "$LIST_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$LIST_RESPONSE"

echo ""
echo "========================================"
echo "Test completed successfully!"
echo "========================================"
