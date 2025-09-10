#include "todo_server_impl.h"
#include <iostream>
#include <algorithm>

TodoServerImpl::TodoServerImpl() : nextId_(1) {
    std::cout << "Todo gRPC Server initialized" << std::endl;
}

Status TodoServerImpl::AddTodo(ServerContext* context, const AddTodoRequest* request,
                              AddTodoResponse* response) {
    std::lock_guard<std::mutex> lock(todoMutex_);
    
    try {
        auto todoItem = std::make_shared<TodoItem>(nextId_++, request->description());
        todoItems_.push_back(todoItem);
        
        // Convert to proto and set response
        *response->mutable_todo_item() = convertToProto(todoItem);
        response->set_success(true);
        response->set_message("Todo item added successfully");
        
        std::cout << "Added todo: " << request->description() << " (ID: " << todoItem->getId() << ")" << std::endl;
        
        return Status::OK;
    } catch (const std::exception& e) {
        response->set_success(false);
        response->set_message("Failed to add todo item: " + std::string(e.what()));
        return Status(grpc::StatusCode::INTERNAL, e.what());
    }
}

Status TodoServerImpl::UpdateTodoStatus(ServerContext* context, const UpdateTodoStatusRequest* request,
                                       UpdateTodoStatusResponse* response) {
    std::lock_guard<std::mutex> lock(todoMutex_);
    
    try {
        auto todoItem = findTodoById(request->id());
        if (!todoItem) {
            response->set_success(false);
            response->set_message("Todo item not found");
            return Status(grpc::StatusCode::NOT_FOUND, "Todo item not found");
        }
        
        todoItem->toggleStatus();
        
        // Convert to proto and set response
        *response->mutable_todo_item() = convertToProto(todoItem);
        response->set_success(true);
        response->set_message("Todo status updated successfully");
        
        std::cout << "Updated todo " << request->id() << " status to: " << todoItem->statusToString() << std::endl;
        
        return Status::OK;
    } catch (const std::exception& e) {
        response->set_success(false);
        response->set_message("Failed to update todo status: " + std::string(e.what()));
        return Status(grpc::StatusCode::INTERNAL, e.what());
    }
}

Status TodoServerImpl::GetTodos(ServerContext* context, const GetTodosRequest* request,
                               GetTodosResponse* response) {
    std::lock_guard<std::mutex> lock(todoMutex_);
    
    try {
        for (const auto& todoItem : todoItems_) {
            *response->add_todo_items() = convertToProto(todoItem);
        }
        
        response->set_success(true);
        response->set_message("Todo items retrieved successfully");
        
        std::cout << "Retrieved " << todoItems_.size() << " todo items" << std::endl;
        
        return Status::OK;
    } catch (const std::exception& e) {
        response->set_success(false);
        response->set_message("Failed to retrieve todo items: " + std::string(e.what()));
        return Status(grpc::StatusCode::INTERNAL, e.what());
    }
}

ProtoTodoItem TodoServerImpl::convertToProto(const std::shared_ptr<TodoItem>& item) {
    ProtoTodoItem protoItem;
    protoItem.set_id(item->getId());
    protoItem.set_description(item->getDescription());
    protoItem.set_status(convertStatus(item->getStatus()));
    return protoItem;
}

ProtoTodoStatus TodoServerImpl::convertStatus(TodoStatus status) {
    return (status == TodoStatus::PENDING) ? 
           todoapp::TodoStatus::PENDING : 
           todoapp::TodoStatus::COMPLETED;
}

std::shared_ptr<TodoItem> TodoServerImpl::findTodoById(int id) {
    auto it = std::find_if(todoItems_.begin(), todoItems_.end(),
                          [id](const std::shared_ptr<TodoItem>& item) {
                              return item->getId() == id;
                          });
    return (it != todoItems_.end()) ? *it : nullptr;
}