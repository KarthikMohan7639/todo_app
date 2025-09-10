using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace TodoClient
{
    public class TodoViewModel : INotifyPropertyChanged
    {
        private readonly TodoService _todoService;
        private string _newTodoDescription = string.Empty;
        private string _statusMessage = "Ready";
        private bool _isLoading = false;

        public TodoViewModel()
        {
            _todoService = new TodoService();
            TodoItems = new ObservableCollection<TodoItem>();
            AddTodoCommand = new RelayCommand(async () => await AddTodoAsync(), () => !string.IsNullOrWhiteSpace(NewTodoDescription));
            RefreshCommand = new RelayCommand(async () => await RefreshTodosAsync());
            ToggleStatusCommand = new RelayCommand<TodoItem>(async (item) => await ToggleStatusAsync(item));
            
            // Load initial data
            _ = Task.Run(async () => await RefreshTodosAsync());
        }

        public ObservableCollection<TodoItem> TodoItems { get; }

        public string NewTodoDescription
        {
            get => _newTodoDescription;
            set
            {
                _newTodoDescription = value;
                OnPropertyChanged(nameof(NewTodoDescription));
                ((RelayCommand)AddTodoCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand AddTodoCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleStatusCommand { get; }

        private async Task AddTodoAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Adding todo...";

                var newTodo = await _todoService.AddTodoAsync(NewTodoDescription);
                TodoItems.Add(newTodo);
                NewTodoDescription = string.Empty;
                StatusMessage = "Todo added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding todo: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshTodosAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading todos...";

                var todos = await _todoService.GetTodosAsync();
                TodoItems.Clear();
                foreach (var todo in todos)
                {
                    TodoItems.Add(todo);
                }
                StatusMessage = $"Loaded {todos.Count} todos";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading todos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleStatusAsync(TodoItem? item)
        {
            if (item == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Updating todo...";

                var updatedTodo = await _todoService.UpdateTodoStatusAsync(item.Id);
                item.IsCompleted = updatedTodo.IsCompleted;
                StatusMessage = "Todo updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating todo: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute();

        public async void Execute(object? parameter) => await _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => parameter is T item && _canExecute(item);

        public async void Execute(object? parameter)
        {
            if (parameter is T item)
                await _execute(item);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}