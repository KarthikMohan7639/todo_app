#!/bin/bash

# Todo App Build Script

echo "Building Todo Application..."
echo "==========================="

# Build C++ Server
echo "Building C++ Server..."
cd server
if [ ! -f "CMakeLists.txt" ]; then
    echo "Error: CMakeLists.txt not found in server directory"
    exit 1
fi

cmake . && make
if [ $? -ne 0 ]; then
    echo "Error: Failed to build C++ server"
    exit 1
fi
echo "✓ C++ Server built successfully"
cd ..

# Build C# Console Client
echo "Building C# Console Client..."
cd client/TodoConsoleClient
if [ ! -f "TodoConsoleClient.csproj" ]; then
    echo "Error: TodoConsoleClient.csproj not found"
    exit 1
fi

dotnet build
if [ $? -ne 0 ]; then
    echo "Error: Failed to build C# console client"
    exit 1
fi
echo "✓ C# Console Client built successfully"
cd ../..

echo ""
echo "Build completed successfully!"
echo ""
echo "To run:"
echo "1. Start server: ./server/todo_server"
echo "2. Start console client: dotnet run --project client/TodoConsoleClient"
echo "3. Or test with curl: curl http://localhost:8080/api/todos"
echo ""
echo "Note: WPF client requires Windows to build and run"