# Build and Test Instructions for Todo gRPC Application

## Prerequisites Setup

### Windows (Recommended for full testing)

1. **Install Visual Studio 2022** with C++ Desktop Development workload
2. **Install vcpkg** package manager:
   ```cmd
   git clone https://github.com/Microsoft/vcpkg.git
   cd vcpkg
   .\bootstrap-vcpkg.bat
   .\vcpkg integrate install
   ```

3. **Install required packages**:
   ```cmd
   .\vcpkg install grpc protobuf abseil --triplet x64-windows
   ```

4. **Install .NET 8.0 SDK** from Microsoft

### Linux (Console client only)

1. **Install gRPC and Protocol Buffers**:
   ```bash
   # Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install -y build-essential cmake
   sudo apt-get install -y libgrpc++-dev libprotobuf-dev protobuf-compiler-grpc
   
   # Or build from source:
   # https://grpc.io/docs/languages/cpp/quickstart/
   ```

2. **Install .NET 8.0 SDK**:
   ```bash
   # Follow instructions at: https://dotnet.microsoft.com/download
   ```

## Building and Testing

### 1. Build C++ gRPC Server

**Windows (Visual Studio):**
```cmd
# Open TodoApp.sln in Visual Studio
# Set TodoServer as startup project
# Build Solution (Ctrl+Shift+B)
# Run (F5)
```

**Linux (CMake):**
```bash
cd TestServer
mkdir build && cd build
cmake ..
make
./todo_test_server
```

### 2. Build and Test C# Clients

**Console Client (Cross-platform):**
```bash
cd TodoConsoleClient
dotnet restore
dotnet build
dotnet run http://localhost:50051
```

**WPF Client (Windows only):**
```cmd
cd TodoClient
dotnet restore
dotnet build
dotnet run
```

## Testing the Application

### 1. Start the gRPC Server
- Server listens on port 50051 by default
- Console output shows server startup and request processing

### 2. Test with Console Client
```
Todo List Console Client (gRPC)
===============================
Connecting to server: http://localhost:50051

Choose an option:
1. List all todos
2. Add a new todo
3. Toggle todo status
4. Exit
```

### 3. Test with WPF Client (Windows)
- Clean GUI with ListView showing todo items
- Add new todos using text input
- Toggle status using checkboxes
- Status bar shows connection and operation status

## Architecture Verification

### Protocol Buffer Schema
The `proto/todo.proto` file defines:
- TodoService with 3 RPC methods
- TodoItem message with id, description, status
- Request/Response messages for each operation

### Generated Code
- C#: Auto-generated during build via Grpc.Tools package
- C++: Generated using protoc and grpc_cpp_plugin

### Key Features Demonstrated
1. **gRPC Communication**: Binary protocol, type-safe
2. **Protocol Buffers**: Efficient serialization
3. **Visual Studio Integration**: Solution-based development
4. **vcpkg Dependency Management**: Modern C++ package management
5. **Cross-platform Client**: Console app works on any OS with .NET

## Expected Behavior

### Server Console Output
```
Todo gRPC Server initialized
Todo gRPC Server listening on 0.0.0.0:50051
Added todo: Buy groceries (ID: 1)
Retrieved 1 todo items
Updated todo 1 status to: Completed
```

### Client Operations
- **Add Todo**: Creates new item with auto-incremented ID
- **List Todos**: Shows all items with current status
- **Toggle Status**: Switches between PENDING and COMPLETED
- **Real-time Updates**: Changes persist across client connections

## Troubleshooting

### Common Issues
1. **Port Already in Use**: Change server port or kill existing process
2. **gRPC Connection Failed**: Verify server is running and accessible
3. **protoc Not Found**: Ensure Protocol Buffers compiler is installed
4. **vcpkg Integration**: Run `vcpkg integrate install` in Visual Studio

### Build Errors
- **C++**: Check vcpkg integration and package installation
- **C#**: Verify .NET 8.0 SDK installation and project restore
- **WPF**: Requires Windows, use console client on other platforms

This architecture provides a modern, efficient, and maintainable solution using industry-standard gRPC and Protocol Buffers technology.