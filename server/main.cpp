#include "todo_server.h"
#include <iostream>
#include <signal.h>
#include <thread>
#include <chrono>
#include <cstdlib>

TodoServer* server = nullptr;

void signalHandler(int signal) {
    if (server) {
        std::cout << "\nShutting down server..." << std::endl;
        server->stop();
        delete server;
        server = nullptr;
    }
    exit(0);
}

int main(int argc, char* argv[]) {
    int port = 8080;
    
    if (argc > 1) {
        port = std::atoi(argv[1]);
    }
    
    // Set up signal handler for graceful shutdown
    signal(SIGINT, signalHandler);
    signal(SIGTERM, signalHandler);
    
    server = new TodoServer(port);
    server->start();
    
    std::cout << "Server running on port " << port << ". Press Ctrl+C to stop." << std::endl;
    
    // Keep the main thread alive
    while (true) {
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }
    
    return 0;
}