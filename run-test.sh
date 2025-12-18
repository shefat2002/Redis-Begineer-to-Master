#!/bin/bash

echo "Starting Project1 Test Console..."
echo ""
echo "Make sure the API is running first:"
echo "  cd Project1.KeyValueStoreAPI"
echo "  docker-compose up -d"
echo "  dotnet run"
echo ""
echo "Press Enter to continue or Ctrl+C to cancel..."
read

cd Project1.TestConsole
dotnet run
