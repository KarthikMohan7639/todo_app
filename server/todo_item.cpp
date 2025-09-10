#include "todo_item.h"
#include <sstream>

TodoItem::TodoItem(int id, const std::string& description)
    : id_(id), description_(description), status_(TodoStatus::PENDING) {
}

void TodoItem::toggleStatus() {
    status_ = (status_ == TodoStatus::PENDING) ? TodoStatus::COMPLETED : TodoStatus::PENDING;
}

std::string TodoItem::statusToString() const {
    return (status_ == TodoStatus::PENDING) ? "Pending" : "Completed";
}

std::string TodoItem::toJson() const {
    std::ostringstream oss;
    oss << "{"
        << "\"id\":" << id_ << ","
        << "\"description\":\"" << description_ << "\","
        << "\"status\":\"" << statusToString() << "\""
        << "}";
    return oss.str();
}