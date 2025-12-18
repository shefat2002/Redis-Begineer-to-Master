#!/bin/bash

# Quick Start Script for SignalR Chat with Redis

echo "🚀 SignalR Chat with Redis - Quick Start"
echo "========================================"
echo ""

# Check if Redis is running
echo "📡 Checking Redis connection..."
if redis-cli ping > /dev/null 2>&1; then
    echo "✅ Redis is running on localhost:6379"
else
    echo "❌ Redis is not running!"
    echo ""
    echo "Please start Redis first:"
    echo "  Option 1 (Docker):  docker run -d -p 6379:6379 redis:latest"
    echo "  Option 2 (Homebrew): brew services start redis"
    echo "  Option 3 (Direct):   redis-server"
    echo ""
    exit 1
fi

echo ""
echo "🔨 Building project..."
cd Project2.SignalRChat
dotnet build --nologo -v quiet

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo ""
    echo "🌐 Starting application..."
    echo ""
    echo "📱 Open your browser to: http://localhost:5000"
    echo ""
    echo "💡 To test scaling, open multiple terminals and run:"
    echo "   Terminal 1: dotnet run --urls http://localhost:5000"
    echo "   Terminal 2: dotnet run --urls http://localhost:5001"
    echo "   Terminal 3: dotnet run --urls http://localhost:5002"
    echo ""
    echo "Press Ctrl+C to stop"
    echo ""
    dotnet run
else
    echo "❌ Build failed!"
    exit 1
fi
