using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoClient
{
    public class TodoViewModel : INotifyPropertyChanged
    {
        private readonly TodoService _todoService;
        private string _newTodoText = string.Empty;

        public TodoViewModel(TodoService todoService)
        {
            _todoService = todoService;
            Todos = new ObservableCollection<TodoItem>();
        }

        public ObservableCollection<TodoItem> Todos { get; }

        public string NewTodoText
        {
            get => _newTodoText;
            set
            {
                _newTodoText = value;
                OnPropertyChanged(nameof(NewTodoText));
            }
        }

        public async Task LoadTodosAsync()
        {
            try
            {
                var todos = await _todoService.GetTodosAsync();
                
                Todos.Clear();
                foreach (var todo in todos)
                {
                    Todos.Add(todo);
                }
            }
            catch (Exception ex)
            {
                // Log error or handle appropriately
                throw new Exception($"Failed to load todos: {ex.Message}", ex);
            }
        }

        public async Task AddTodoAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTodoText))
                return;

            try
            {
                var newTodo = await _todoService.AddTodoAsync(NewTodoText);
                Todos.Add(newTodo);
                NewTodoText = string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add todo: {ex.Message}", ex);
            }
        }

        public async Task UpdateTodoStatusAsync(int todoId)
        {
            try
            {
                await _todoService.UpdateTodoStatusAsync(todoId);
                
                // Update the local item
                var todo = Todos.FirstOrDefault(t => t.Id == todoId);
                if (todo != null)
                {
                    todo.IsCompleted = !todo.IsCompleted;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update todo status: {ex.Message}", ex);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}