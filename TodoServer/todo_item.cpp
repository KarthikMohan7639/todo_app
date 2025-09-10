#include "todo_item.h"

TodoItem::TodoItem(int id, const std::string& description) 
    : id_(id), description_(description), status_(TodoStatus::PENDING) {
}

void TodoItem::toggleStatus() {
    status_ = (status_ == TodoStatus::PENDING) ? TodoStatus::COMPLETED : TodoStatus::PENDING;
}

std::string TodoItem::statusToString() const {
    return (status_ == TodoStatus::PENDING) ? "Pending" : "Completed";
}