using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TodoClient
{
    public partial class MainWindow : Window
    {
        private readonly TodoService _todoService;
        private readonly TodoViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            _todoService = new TodoService("http://localhost:8080");
            _viewModel = new TodoViewModel(_todoService);
            
            DataContext = _viewModel;
            
            // Set up event handlers
            _todoService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _todoService.TodoListUpdated += OnTodoListUpdated;
            
            // Connect to server and load initial data
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                await _viewModel.LoadTodosAsync();
                UpdateConnectionStatus(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}", "Connection Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateConnectionStatus(false);
            }
        }

        private void OnConnectionStatusChanged(object? sender, bool isConnected)
        {
            Dispatcher.Invoke(() => UpdateConnectionStatus(isConnected));
        }

        private void OnTodoListUpdated(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(async () => await _viewModel.LoadTodosAsync());
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            StatusText.Text = isConnected ? "Connected" : "Disconnected";
            StatusText.Foreground = isConnected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }

        private async void AddTodoButton_Click(object sender, RoutedEventArgs e)
        {
            await AddNewTodo();
        }

        private async void NewTodoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddNewTodo();
            }
        }

        private async Task AddNewTodo()
        {
            try
            {
                await _viewModel.AddTodoAsync();
                NewTodoTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add todo: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TodoItem_StatusChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is int todoId)
            {
                try
                {
                    await _viewModel.UpdateTodoStatusAsync(todoId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update todo status: {ex.Message}", "Error", 
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Revert the checkbox state
                    checkBox.IsChecked = !checkBox.IsChecked;
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadTodosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh todo list: {ex.Message}", "Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _todoService?.Dispose();
            base.OnClosed(e);
        }
    }
}