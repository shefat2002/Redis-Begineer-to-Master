#!/bin/bash

# Project 1 Testing Script
# Tests all endpoints of the Key-Value Store API

API_URL="http://localhost:5000/api/users"
USER_ID=""

echo "🧪 Testing Project 1: Key-Value Store API"
echo "=========================================="
echo ""

# Test 1: Create User
echo "📝 Test 1: Creating a new user..."
RESPONSE=$(curl -s -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "dateOfBirth": "1990-01-15T00:00:00Z",
    "phoneNumber": "+1234567890",
    "address": "123 Main Street"
  }')

USER_ID=$(echo $RESPONSE | grep -o '"id":"[^"]*' | grep -o '[^"]*$')
echo "✅ User created with ID: $USER_ID"
echo "Response: $RESPONSE"
echo ""

# Test 2: Get User
echo "📖 Test 2: Getting user by ID..."
curl -s -X GET "$API_URL/$USER_ID" | jq '.'
echo ""

# Test 3: Check TTL
echo "⏱️  Test 3: Checking TTL (should be -1 for no expiration)..."
curl -s -X GET "$API_URL/$USER_ID/ttl" | jq '.'
echo ""

# Test 4: Check Existence
echo "🔍 Test 4: Checking if user exists..."
curl -s -X GET "$API_URL/$USER_ID/exists" | jq '.'
echo ""

# Test 5: Update User
echo "✏️  Test 5: Updating user..."
curl -s -X PUT "$API_URL/$USER_ID" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": \"$USER_ID\",
    \"firstName\": \"John\",
    \"lastName\": \"Smith\",
    \"email\": \"john.smith@example.com\",
    \"dateOfBirth\": \"1990-01-15T00:00:00Z\",
    \"phoneNumber\": \"+1234567890\",
    \"address\": \"456 New Avenue\"
  }" | jq '.'
echo ""

# Test 6: Create User with TTL
echo "⏰ Test 6: Creating user with 5-minute TTL..."
RESPONSE=$(curl -s -X POST "$API_URL?ttlMinutes=5" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "dateOfBirth": "1992-05-20T00:00:00Z",
    "phoneNumber": "+0987654321",
    "address": "789 Oak Street"
  }')

USER_ID_2=$(echo $RESPONSE | grep -o '"id":"[^"]*' | grep -o '[^"]*$')
echo "✅ User created with ID: $USER_ID_2"
echo ""

# Test 7: Check TTL for new user
echo "⏱️  Test 7: Checking TTL for user with expiration..."
curl -s -X GET "$API_URL/$USER_ID_2/ttl" | jq '.'
echo ""

# Test 8: Delete First User
echo "🗑️  Test 8: Deleting first user..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$API_URL/$USER_ID")
echo "HTTP Status: $HTTP_CODE"
if [ "$HTTP_CODE" = "204" ]; then
    echo "✅ User deleted successfully"
else
    echo "❌ Failed to delete user"
fi
echo ""

# Test 9: Verify Deletion
echo "🔍 Test 9: Verifying first user is deleted..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_URL/$USER_ID")
if [ "$HTTP_CODE" = "404" ]; then
    echo "✅ User not found (as expected)"
else
    echo "❌ User still exists (unexpected)"
fi
echo ""

# Test 10: Try to create duplicate
echo "🔄 Test 10: Attempting to create duplicate user (should fail)..."
RESPONSE=$(curl -s -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": \"$USER_ID_2\",
    \"firstName\": \"Duplicate\",
    \"lastName\": \"User\",
    \"email\": \"dup@example.com\",
    \"dateOfBirth\": \"1990-01-01T00:00:00Z\",
    \"phoneNumber\": \"+1111111111\",
    \"address\": \"999 Dup St\"
  }")
echo "$RESPONSE" | jq '.'
echo ""

# Cleanup
echo "🧹 Cleanup: Deleting second user..."
curl -s -X DELETE "$API_URL/$USER_ID_2" > /dev/null
echo "✅ Cleanup complete"
echo ""

echo "=========================================="
echo "✨ All tests completed!"
echo ""
echo "💡 Tips:"
echo "  - View Redis data at http://localhost:8081"
echo "  - Monitor Redis: docker exec -it project1-redis redis-cli MONITOR"
echo "  - Check keys: docker exec -it project1-redis redis-cli KEYS 'user:*'"
