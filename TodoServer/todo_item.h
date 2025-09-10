#ifndef TODO_ITEM_H
#define TODO_ITEM_H

#include <string>
#include <memory>

enum class TodoStatus {
    PENDING = 0,
    COMPLETED = 1
};

class TodoItem {
public:
    TodoItem(int id, const std::string& description);
    
    int getId() const { return id_; }
    const std::string& getDescription() const { return description_; }
    TodoStatus getStatus() const { return status_; }
    
    void setStatus(TodoStatus status) { status_ = status; }
    void toggleStatus();
    
    std::string statusToString() const;

private:
    int id_;
    std::string description_;
    TodoStatus status_;
};

#endif // TODO_ITEM_H