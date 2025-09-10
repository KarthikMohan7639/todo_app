#ifndef TODO_SERVER_H
#define TODO_SERVER_H

#include "todo_item.h"
#include <vector>
#include <mutex>
#include <memory>
#include <string>
#include <thread>
#include <set>

class TodoServer {
public:
    TodoServer(int port = 8080);
    ~TodoServer();
    
    void start();
    void stop();
    
private:
    // Todo management
    std::shared_ptr<TodoItem> addTodoItem(const std::string& description);
    bool updateTodoStatus(int id);
    std::string getTodoListJson();
    
    // HTTP handling
    void handleClient(int clientSocket);
    std::string handleRequest(const std::string& request);
    std::string parseHttpBody(const std::string& request);
    std::string createHttpResponse(const std::string& body, const std::string& contentType = "application/json");
    
    // WebSocket handling (simplified notification system)
    void notifyClients(const std::string& message);
    void addWebSocketClient(int clientSocket);
    void removeWebSocketClient(int clientSocket);
    
    // Server state
    int port_;
    int serverSocket_;
    bool running_;
    std::thread serverThread_;
    
    // Todo data
    std::vector<std::shared_ptr<TodoItem>> todoItems_;
    std::mutex todoMutex_;
    int nextId_;
    
    // WebSocket clients for notifications
    std::set<int> webSocketClients_;
    std::mutex clientsMutex_;
};

#endif // TODO_SERVER_H