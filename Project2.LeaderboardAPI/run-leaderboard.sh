#!/bin/bash

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   🎮 Leaderboard API - Quick Start                ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════╝${NC}\n"

# Check Redis
echo -e "${YELLOW}1️⃣  Checking Redis...${NC}"
if redis-cli ping > /dev/null 2>&1; then
    echo -e "${GREEN}   ✅ Redis is running${NC}\n"
else
    echo -e "${RED}   ❌ Redis is not running${NC}"
    echo -e "${YELLOW}   Starting Redis with Docker...${NC}"
    docker run -d --name redis-leaderboard -p 6379:6379 redis:7-alpine
    sleep 2
    if redis-cli ping > /dev/null 2>&1; then
        echo -e "${GREEN}   ✅ Redis started successfully${NC}\n"
    else
        echo -e "${RED}   ❌ Failed to start Redis. Please install Redis or Docker.${NC}"
        exit 1
    fi
fi

# Build project
echo -e "${YELLOW}2️⃣  Building project...${NC}"
cd Project2.LeaderboardAPI
if dotnet build > /dev/null 2>&1; then
    echo -e "${GREEN}   ✅ Build successful${NC}\n"
else
    echo -e "${RED}   ❌ Build failed${NC}"
    exit 1
fi

# Start API
echo -e "${YELLOW}3️⃣  Starting API...${NC}"
echo -e "${BLUE}   🌐 API: http://localhost:5000${NC}"
echo -e "${BLUE}   📖 OpenAPI: http://localhost:5000/openapi/v1.json${NC}\n"

echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}   API is starting... Press Ctrl+C to stop${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"

dotnet run
