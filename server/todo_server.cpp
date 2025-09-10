#include "todo_server.h"
#include <iostream>
#include <sstream>
#include <algorithm>
#include <cstring>

#ifdef _WIN32
    #include <winsock2.h>
    #include <ws2tcpip.h>
    #pragma comment(lib, "ws2_32.lib")
#else
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #include <fcntl.h>
    #define closesocket close
#endif

TodoServer::TodoServer(int port) : port_(port), serverSocket_(-1), running_(false), nextId_(1) {
#ifdef _WIN32
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
#endif
}

TodoServer::~TodoServer() {
    stop();
#ifdef _WIN32
    WSACleanup();
#endif
}

void TodoServer::start() {
    if (running_) return;
    
    // Create socket
    serverSocket_ = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket_ == -1) {
        std::cerr << "Failed to create socket" << std::endl;
        return;
    }
    
    // Set socket options
    int opt = 1;
    setsockopt(serverSocket_, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt));
    
    // Bind socket
    struct sockaddr_in address;
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_port = htons(port_);
    
    if (bind(serverSocket_, (struct sockaddr*)&address, sizeof(address)) < 0) {
        std::cerr << "Failed to bind socket to port " << port_ << std::endl;
        closesocket(serverSocket_);
        return;
    }
    
    // Listen for connections
    if (listen(serverSocket_, 10) < 0) {
        std::cerr << "Failed to listen on socket" << std::endl;
        closesocket(serverSocket_);
        return;
    }
    
    running_ = true;
    std::cout << "Todo server started on port " << port_ << std::endl;
    
    // Start server thread
    serverThread_ = std::thread([this]() {
        while (running_) {
            struct sockaddr_in clientAddress;
            socklen_t clientAddressLength = sizeof(clientAddress);
            
            int clientSocket = accept(serverSocket_, (struct sockaddr*)&clientAddress, &clientAddressLength);
            if (clientSocket >= 0) {
                std::thread clientThread(&TodoServer::handleClient, this, clientSocket);
                clientThread.detach();
            }
        }
    });
}

void TodoServer::stop() {
    if (!running_) return;
    
    running_ = false;
    
    if (serverSocket_ != -1) {
        closesocket(serverSocket_);
        serverSocket_ = -1;
    }
    
    if (serverThread_.joinable()) {
        serverThread_.join();
    }
    
    std::cout << "Todo server stopped" << std::endl;
}

void TodoServer::handleClient(int clientSocket) {
    char buffer[4096];
    
    while (running_) {
        memset(buffer, 0, sizeof(buffer));
        int bytesReceived = recv(clientSocket, buffer, sizeof(buffer) - 1, 0);
        
        if (bytesReceived <= 0) {
            break;
        }
        
        std::string request(buffer);
        std::string response = handleRequest(request);
        
        send(clientSocket, response.c_str(), response.length(), 0);
        
        // For simplicity, close connection after each request
        // In a real implementation, you'd handle keep-alive connections
        break;
    }
    
    closesocket(clientSocket);
}

std::string TodoServer::handleRequest(const std::string& request) {
    std::istringstream iss(request);
    std::string method, path, httpVersion;
    iss >> method >> path >> httpVersion;
    
    // Enable CORS
    std::string corsHeaders = "Access-Control-Allow-Origin: *\r\n"
                             "Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\r\n"
                             "Access-Control-Allow-Headers: Content-Type\r\n";
    
    // Handle OPTIONS preflight requests
    if (method == "OPTIONS") {
        return "HTTP/1.1 200 OK\r\n" + corsHeaders + "\r\n";
    }
    
    try {
        if (method == "GET" && path == "/api/todos") {
            std::string jsonResponse = getTodoListJson();
            return createHttpResponse(jsonResponse);
        }
        else if (method == "POST" && path == "/api/todos") {
            std::string body = parseHttpBody(request);
            
            // Simple JSON parsing - look for description field
            size_t descPos = body.find("\"Description\":");
            if (descPos == std::string::npos) {
                descPos = body.find("\"description\":");
            }
            
            if (descPos != std::string::npos) {
                size_t startQuote = body.find("\"", descPos + 14);
                size_t endQuote = body.find("\"", startQuote + 1);
                
                if (startQuote != std::string::npos && endQuote != std::string::npos) {
                    std::string description = body.substr(startQuote + 1, endQuote - startQuote - 1);
                    auto newItem = addTodoItem(description);
                    
                    // Notify all connected clients
                    notifyClients("ITEM_ADDED:" + newItem->toJson());
                    
                    return createHttpResponse(newItem->toJson());
                }
            }
            
            return createHttpResponse("{\"error\":\"Invalid request body\"}", "application/json", "400 Bad Request");
        }
        else if (method == "PUT" && path.find("/api/todos/") == 0) {
            // Extract ID from path
            std::string idStr = path.substr(11); // "/api/todos/".length() = 11
            int id = std::stoi(idStr);
            
            if (updateTodoStatus(id)) {
                std::string jsonResponse = getTodoListJson();
                
                // Notify all connected clients
                notifyClients("ITEM_UPDATED:" + std::to_string(id));
                
                return createHttpResponse("{\"success\":true}");
            } else {
                return createHttpResponse("{\"error\":\"Todo item not found\"}", "application/json");
            }
        }
    }
    catch (const std::exception& e) {
        return createHttpResponse("{\"error\":\"Internal server error\"}", "application/json");
    }
    
    // 404 Not Found
    return "HTTP/1.1 404 Not Found\r\n" + corsHeaders + "Content-Type: application/json\r\n\r\n{\"error\":\"Not found\"}";
}

std::string TodoServer::parseHttpBody(const std::string& request) {
    size_t bodyStart = request.find("\r\n\r\n");
    if (bodyStart != std::string::npos) {
        return request.substr(bodyStart + 4);
    }
    return "";
}

std::string TodoServer::createHttpResponse(const std::string& body, const std::string& contentType, const std::string& status) {
    std::ostringstream response;
    response << "HTTP/1.1 " << status << "\r\n"
             << "Access-Control-Allow-Origin: *\r\n"
             << "Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\r\n"
             << "Access-Control-Allow-Headers: Content-Type\r\n"
             << "Content-Type: " << contentType << "\r\n"
             << "Content-Length: " << body.length() << "\r\n"
             << "\r\n"
             << body;
    return response.str();
}

std::shared_ptr<TodoItem> TodoServer::addTodoItem(const std::string& description) {
    std::lock_guard<std::mutex> lock(todoMutex_);
    auto item = std::make_shared<TodoItem>(nextId_++, description);
    todoItems_.push_back(item);
    return item;
}

bool TodoServer::updateTodoStatus(int id) {
    std::lock_guard<std::mutex> lock(todoMutex_);
    for (auto& item : todoItems_) {
        if (item->getId() == id) {
            item->toggleStatus();
            return true;
        }
    }
    return false;
}

std::string TodoServer::getTodoListJson() {
    std::lock_guard<std::mutex> lock(todoMutex_);
    std::ostringstream oss;
    oss << "[";
    
    for (size_t i = 0; i < todoItems_.size(); ++i) {
        if (i > 0) oss << ",";
        oss << todoItems_[i]->toJson();
    }
    
    oss << "]";
    return oss.str();
}

void TodoServer::notifyClients(const std::string& message) {
    // Simplified notification - in a real implementation, this would use WebSockets
    std::cout << "Notification: " << message << std::endl;
}

void TodoServer::addWebSocketClient(int clientSocket) {
    std::lock_guard<std::mutex> lock(clientsMutex_);
    webSocketClients_.insert(clientSocket);
}

void TodoServer::removeWebSocketClient(int clientSocket) {
    std::lock_guard<std::mutex> lock(clientsMutex_);
    webSocketClients_.erase(clientSocket);
}