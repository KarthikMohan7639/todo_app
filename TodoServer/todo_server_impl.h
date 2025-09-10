#ifndef TODO_SERVER_IMPL_H
#define TODO_SERVER_IMPL_H

#include <grpcpp/grpcpp.h>
#include "../proto/todo.grpc.pb.h"
#include "todo_item.h"
#include <vector>
#include <mutex>
#include <memory>

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using todoapp::TodoService;
using todoapp::AddTodoRequest;
using todoapp::AddTodoResponse;
using todoapp::UpdateTodoStatusRequest;
using todoapp::UpdateTodoStatusResponse;
using todoapp::GetTodosRequest;
using todoapp::GetTodosResponse;
using todoapp::TodoItem as ProtoTodoItem;
using todoapp::TodoStatus as ProtoTodoStatus;

class TodoServerImpl final : public TodoService::Service {
public:
    TodoServerImpl();
    
    Status AddTodo(ServerContext* context, const AddTodoRequest* request,
                   AddTodoResponse* response) override;
    
    Status UpdateTodoStatus(ServerContext* context, const UpdateTodoStatusRequest* request,
                           UpdateTodoStatusResponse* response) override;
    
    Status GetTodos(ServerContext* context, const GetTodosRequest* request,
                    GetTodosResponse* response) override;

private:
    std::vector<std::shared_ptr<TodoItem>> todoItems_;
    std::mutex todoMutex_;
    int nextId_;
    
    // Helper methods
    ProtoTodoItem convertToProto(const std::shared_ptr<TodoItem>& item);
    ProtoTodoStatus convertStatus(TodoStatus status);
    std::shared_ptr<TodoItem> findTodoById(int id);
};

#endif // TODO_SERVER_IMPL_H