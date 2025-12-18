#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

API_URL="http://localhost:5000/api/leaderboard"

echo -e "${BLUE}🎮 Testing Leaderboard API${NC}\n"

# Check if Redis is running
echo -e "${BLUE}1. Checking Redis connection...${NC}"
if redis-cli ping > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Redis is running${NC}\n"
else
    echo -e "${RED}❌ Redis is not running. Please start Redis first.${NC}"
    echo "Run: docker run -d -p 6379:6379 redis:7-alpine"
    exit 1
fi

# Clear previous leaderboard data
echo -e "${BLUE}2. Clearing previous leaderboard data...${NC}"
redis-cli DEL "leaderboard:global" > /dev/null
redis-cli KEYS "player:stats:*" | xargs -r redis-cli DEL > /dev/null
echo -e "${GREEN}✅ Data cleared${NC}\n"

# Start the API in background
echo -e "${BLUE}3. Starting the API...${NC}"
dotnet run > /dev/null 2>&1 &
API_PID=$!
sleep 5
echo -e "${GREEN}✅ API started (PID: $API_PID)${NC}\n"

# Submit test scores
echo -e "${BLUE}4. Submitting player scores...${NC}"

players=("alice:1500" "bob:2000" "charlie:1800" "david:2500" "eve:1200" "frank:3000" "grace:2200" "henry:1900" "ivy:2700" "jack:1600")

for player in "${players[@]}"; do
    IFS=':' read -r name score <<< "$player"
    response=$(curl -s -X POST "$API_URL/score" \
      -H "Content-Type: application/json" \
      -d "{\"playerId\":\"$name\",\"score\":$score}")
    echo "  ✓ $name: $score points"
done
echo -e "${GREEN}✅ All scores submitted${NC}\n"

# Test 1: Get Top 10
echo -e "${BLUE}5. Test: Get Top 10 Players${NC}"
curl -s "$API_URL/top/10" | python3 -m json.tool
echo ""

# Test 2: Get Player Rank
echo -e "${BLUE}6. Test: Get Alice's Rank${NC}"
curl -s "$API_URL/rank/alice" | python3 -m json.tool
echo ""

# Test 3: Get Player Stats
echo -e "${BLUE}7. Test: Get Alice's Stats${NC}"
curl -s "$API_URL/player/alice" | python3 -m json.tool
echo ""

# Test 4: Submit Updated Score
echo -e "${BLUE}8. Test: Update Alice's Score to 2800${NC}"
curl -s -X POST "$API_URL/score" \
  -H "Content-Type: application/json" \
  -d '{"playerId":"alice","score":2800}' | python3 -m json.tool
echo ""

# Test 5: Get Updated Rank
echo -e "${BLUE}9. Test: Get Alice's New Rank${NC}"
curl -s "$API_URL/rank/alice" | python3 -m json.tool
echo ""

# Test 6: Get Range
echo -e "${BLUE}10. Test: Get Players Ranked 3-5${NC}"
curl -s "$API_URL/range/3/5" | python3 -m json.tool
echo ""

# Test 7: Get Stats
echo -e "${BLUE}11. Test: Get Leaderboard Statistics${NC}"
curl -s "$API_URL/stats" | python3 -m json.tool
echo ""

# View Redis Data
echo -e "${BLUE}12. Redis Data Verification${NC}"
echo "Leaderboard (Top 5):"
redis-cli ZREVRANGE leaderboard:global 0 4 WITHSCORES
echo ""
echo "Alice's Stats:"
redis-cli HGETALL player:stats:alice
echo ""

# Cleanup
echo -e "${BLUE}13. Cleaning up...${NC}"
kill $API_PID
echo -e "${GREEN}✅ API stopped${NC}"

echo -e "\n${GREEN}🎉 All tests completed!${NC}"
