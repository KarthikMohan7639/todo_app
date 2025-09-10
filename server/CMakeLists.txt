cmake_minimum_required(VERSION 3.10)
project(TodoServer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find required packages
find_package(Threads REQUIRED)

# Include directories
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

# Add executable
add_executable(todo_server
    main.cpp
    todo_server.cpp
    todo_item.cpp
)

# Link libraries
target_link_libraries(todo_server 
    Threads::Threads
)

# Platform-specific libraries
if(WIN32)
    target_link_libraries(todo_server ws2_32 wsock32)
elseif(UNIX)
    target_link_libraries(todo_server)
endif()