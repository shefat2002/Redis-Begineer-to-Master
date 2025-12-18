#!/bin/bash

echo "🐳 SignalR Chat with Redis - Docker Compose Startup"
echo "===================================================="
echo ""

# Check if docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed!"
    echo "Please install Docker Desktop: https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Check if docker-compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose is not installed!"
    exit 1
fi

echo "✅ Docker is installed"
echo ""

# Determine docker compose command
if docker compose version &> /dev/null 2>&1; then
    DOCKER_COMPOSE="docker compose"
else
    DOCKER_COMPOSE="docker-compose"
fi

echo "🔨 Building and starting services..."
echo ""

$DOCKER_COMPOSE up -d --build

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ All services started successfully!"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "🌐 Services Available:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "📱 SignalR Chat Application:"
    echo "   http://localhost:5000"
    echo ""
    echo "🔴 Redis Commander (GUI):"
    echo "   http://localhost:8081"
    echo ""
    echo "🔧 Redis Server:"
    echo "   localhost:6379"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "💡 Usage:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "1. Open http://localhost:5000 in your browser"
    echo "2. Open http://localhost:8081 to monitor Redis"
    echo "3. Start chatting and watch Redis Commander!"
    echo ""
    echo "🔍 View logs:"
    echo "   $DOCKER_COMPOSE logs -f"
    echo ""
    echo "🛑 Stop services:"
    echo "   $DOCKER_COMPOSE down"
    echo ""
    echo "🧹 Stop and remove volumes:"
    echo "   $DOCKER_COMPOSE down -v"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
else
    echo "❌ Failed to start services!"
    exit 1
fi
