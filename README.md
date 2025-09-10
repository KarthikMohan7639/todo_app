# Todo List Application

This is a client-server todo list application with a C++ server and C# WPF client as specified in the requirements.

## Architecture

### C++ Server
- **Technology**: Pure C++ with standard HTTP libraries
- **Port**: 8080 (configurable)
- **API Endpoints**:
  - `GET /api/todos` - Get all todo items
  - `POST /api/todos` - Add a new todo item (JSON body: `{"description": "item description"}`)
  - `PUT /api/todos/{id}` - Toggle status of todo item by ID
- **Features**:
  - In-memory storage of todo items
  - Thread-safe operations
  - CORS support for web clients
  - Real-time notifications (via console logging for demo)

### C# WPF Client
- **Technology**: .NET 8.0 WPF Application
- **Features**:
  - Clean UI with ListView showing todo items
  - Checkboxes for status management
  - Add new todo items via text input
  - Real-time updates (manual refresh for demo)
  - Connection status indicator

### Data Model
Each todo item has:
- `id`: Unique integer identifier (auto-generated)
- `description`: String description of the task
- `status`: "Pending" or "Completed"

## Building and Running

### Prerequisites
- **For Server**: C++ compiler with C++17 support, CMake 3.10+
- **For Client**: .NET 8.0 SDK (Windows required for WPF)

### C++ Server
```bash
cd server
cmake .
make
./todo_server [port]
```

### C# WPF Client (Windows only)
```bash
cd client/TodoClient
dotnet restore
dotnet run
```

### Console Client (Cross-platform testing)
```bash
cd client/TodoConsoleClient
dotnet run [server_url]
```

## Testing

### Manual Testing with curl
```bash
# Get all todos
curl http://localhost:8080/api/todos

# Add a new todo
curl -X POST http://localhost:8080/api/todos \
  -H "Content-Type: application/json" \
  -d '{"description":"Buy groceries"}'

# Toggle todo status
curl -X PUT http://localhost:8080/api/todos/1
```

### Integration Testing
1. Start the C++ server: `./server/todo_server`
2. Run the console client: `dotnet run --project client/TodoConsoleClient`
3. Test operations: add items, list items, toggle status

## Features Implemented

### ✅ C++ Server Requirements
- [x] Network communication with HTTP REST API
- [x] Add Item endpoint with auto-generated ID
- [x] Update Status endpoint for toggling
- [x] Get List endpoint for retrieving all items
- [x] Real-time notifications (console logging)
- [x] In-memory data management with thread safety

### ✅ C# Client Requirements  
- [x] WPF UI with ListView and CheckBoxes
- [x] Input field and Add button
- [x] Connect and fetch initial todo list
- [x] Add new items via server API
- [x] Toggle status via UI interactions
- [x] Manual refresh capability

### 📝 Implementation Notes
- Real-time synchronization is implemented via notifications in the server console for demo purposes
- Full WebSocket implementation would require additional libraries
- WPF client requires Windows; console client provided for cross-platform testing
- CORS headers added for potential web client compatibility
- Thread-safe server implementation supports multiple concurrent clients

## File Structure
```
├── server/
│   ├── CMakeLists.txt
│   ├── main.cpp
│   ├── todo_server.h
│   ├── todo_server.cpp
│   ├── todo_item.h
│   └── todo_item.cpp
├── client/
│   ├── TodoClient/           # WPF Application (Windows)
│   │   ├── TodoClient.csproj
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── Models.cs
│   │   ├── TodoViewModel.cs
│   │   ├── TodoService.cs
│   │   └── Program.cs
│   └── TodoConsoleClient/    # Console Client (Cross-platform)
│       ├── TodoConsoleClient.csproj
│       └── Program.cs
└── README.md
```