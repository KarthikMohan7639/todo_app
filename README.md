# Todo List Application - gRPC Version

This is a client-server todo list application with a C++ gRPC server and C# WPF client using Visual Studio solution structure.

## Architecture

### C++ gRPC Server
- **Technology**: C++ with gRPC and Protocol Buffers
- **Port**: 50051 (gRPC default)
- **Services**:
  - `AddTodo` - Add a new todo item (protobuf message: `AddTodoRequest`)
  - `UpdateTodoStatus` - Toggle status of todo item by ID (protobuf message: `UpdateTodoStatusRequest`)
  - `GetTodos` - Get all todo items (protobuf message: `GetTodosRequest`)
- **Features**:
  - In-memory storage of todo items
  - Thread-safe operations using gRPC's built-in thread safety
  - Protocol Buffers for efficient serialization
  - Real-time notifications via console logging

### C# WPF Client
- **Technology**: .NET 8.0 WPF Application with gRPC.Net.Client
- **Features**:
  - Clean UI with ListView showing todo items
  - Checkboxes for status management
  - Add new todo items via text input
  - gRPC communication with the server
  - Connection status indicator
  - Async/await pattern for non-blocking UI

### Console Client
- **Technology**: .NET 8.0 Console Application with gRPC.Net.Client
- **Features**:
  - Interactive command-line interface
  - Cross-platform testing capability
  - Full CRUD operations via gRPC

### Data Model
Each todo item has:
- `id`: Unique integer identifier (auto-generated)
- `description`: String description of the task  
- `status`: `PENDING` or `COMPLETED` (Protocol Buffers enum)

## Building and Running

### Prerequisites
- **For Server**: 
  - Visual Studio 2019/2022 with C++ workload
  - vcpkg package manager
  - CMake 3.15+ (if building outside Visual Studio)
- **For Client**: 
  - .NET 8.0 SDK
  - Visual Studio 2019/2022 (optional, for WPF client on Windows)

### Setup vcpkg (Required for C++ Server)
```bash
# Clone vcpkg if not already installed
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
.\bootstrap-vcpkg.bat  # Windows
# or
./bootstrap-vcpkg.sh   # Linux/Mac

# Install required packages
./vcpkg install grpc protobuf abseil --triplet x64-windows
# or for Linux
./vcpkg install grpc protobuf abseil --triplet x64-linux

# Integrate with Visual Studio
./vcpkg integrate install
```

### Generate Protocol Buffer Files
Before building, you need to generate the gRPC and Protocol Buffer files:

```bash
# Generate C++ files (from project root)
protoc --grpc_out=./proto --cpp_out=./proto --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` ./proto/todo.proto

# C# files are generated automatically by the .NET build system via Grpc.Tools package
```

### Building with Visual Studio
1. Open `TodoApp.sln` in Visual Studio
2. Make sure vcpkg is properly integrated
3. Set the solution configuration to Release or Debug
4. Build Solution (Ctrl+Shift+B)

### Building with Command Line

#### C++ Server (Alternative method)
```bash
cd TodoServer
# Configure vcpkg toolchain
cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=[vcpkg-root]/scripts/buildsystems/vcpkg.cmake
cmake --build build --config Release
```

#### C# WPF Client
```bash
cd TodoClient
dotnet restore
dotnet build
dotnet run
```

#### Console Client
```bash
cd TodoConsoleClient
dotnet restore
dotnet build
dotnet run [server_url]
```

### Running the Application
1. **Start the gRPC Server**:
   ```bash
   # From Visual Studio: Set TodoServer as startup project and run
   # Or from command line:
   cd TodoServer/build/Release  # or Debug
   ./TodoServer.exe
   ```
   
2. **Start the WPF Client** (Windows only):
   ```bash
   cd TodoClient
   dotnet run
   ```

3. **Or use Console Client** (Cross-platform):
   ```bash
   cd TodoConsoleClient
   dotnet run http://localhost:50051
   ```

## Testing

### Manual Testing with Console Client
The console client provides an interactive interface:
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

### Integration Testing
1. Start the C++ gRPC server
2. Run the console client: `dotnet run --project TodoConsoleClient`
3. Test operations: add items, list items, toggle status
4. Verify all operations work correctly

## Features Implemented

### ✅ C++ gRPC Server Requirements
- [x] gRPC service with Protocol Buffers
- [x] AddTodo RPC with auto-generated ID
- [x] UpdateTodoStatus RPC for toggling
- [x] GetTodos RPC for retrieving all items
- [x] Thread-safe in-memory data management
- [x] Real-time notifications (console logging)
- [x] Visual Studio project structure
- [x] vcpkg package management

### ✅ C# Client Requirements  
- [x] WPF UI with ListView and CheckBoxes
- [x] gRPC.Net.Client for server communication
- [x] Protocol Buffers message handling
- [x] Input field and Add button
- [x] Connect and fetch initial todo list
- [x] Add new items via gRPC service
- [x] Toggle status via UI interactions
- [x] Async/await for non-blocking operations

### ✅ Additional Features
- [x] Cross-platform console client
- [x] Visual Studio solution structure
- [x] vcpkg integration for dependency management
- [x] Protocol Buffers for efficient serialization
- [x] Modern C# async patterns
- [x] Comprehensive error handling

## Protocol Buffer Schema
The application uses the following Protocol Buffer definition (`proto/todo.proto`):
```protobuf
service TodoService {
  rpc AddTodo(AddTodoRequest) returns (AddTodoResponse);
  rpc UpdateTodoStatus(UpdateTodoStatusRequest) returns (UpdateTodoStatusResponse);
  rpc GetTodos(GetTodosRequest) returns (GetTodosResponse);
}

message TodoItem {
  int32 id = 1;
  string description = 2;
  TodoStatus status = 3;
}

enum TodoStatus {
  PENDING = 0;
  COMPLETED = 1;
}
```

## File Structure
```
├── TodoApp.sln                    # Visual Studio Solution
├── vcpkg.json                     # vcpkg manifest for dependencies
├── proto/
│   └── todo.proto                 # Protocol Buffer service definition
├── TodoServer/                    # C++ gRPC Server (Visual Studio project)
│   ├── TodoServer.vcxproj
│   ├── main.cpp
│   ├── todo_server_impl.h
│   ├── todo_server_impl.cpp
│   ├── todo_item.h
│   └── todo_item.cpp
├── TodoClient/                    # C# WPF Application
│   ├── TodoClient.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Models.cs
│   ├── TodoViewModel.cs
│   └── TodoService.cs
├── TodoConsoleClient/             # C# Console Client
│   ├── TodoConsoleClient.csproj
│   └── Program.cs
└── README.md
```

## Development Notes
- The solution is designed for Visual Studio with vcpkg integration
- Protocol Buffer files are automatically generated during build
- gRPC provides better performance than HTTP REST for internal services
- The WPF client uses MVVM pattern for clean separation of concerns
- All gRPC calls are asynchronous for better responsiveness