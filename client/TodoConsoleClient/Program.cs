using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace TodoConsoleClient
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }

    public class AddTodoRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    public class TodoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TodoApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<List<TodoItem>> GetTodosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/todos");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TodoItem>>(json) ?? new List<TodoItem>();
        }

        public async Task<TodoItem> AddTodoAsync(string description)
        {
            var request = new AddTodoRequest { Description = description };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/todos", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TodoItem>(responseJson) 
                ?? throw new Exception("Invalid response from server");
        }

        public async Task UpdateTodoStatusAsync(int todoId)
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/todos/{todoId}", null);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    class Program
    {
        private static TodoApiClient? _apiClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Todo Console Client");
            Console.WriteLine("==================");

            string serverUrl = args.Length > 0 ? args[0] : "http://localhost:8080";
            _apiClient = new TodoApiClient(serverUrl);

            Console.WriteLine($"Connecting to server at: {serverUrl}");
            Console.WriteLine();

            try
            {
                await TestConnection();
                await ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _apiClient?.Dispose();
            }
        }

        private static async Task TestConnection()
        {
            try
            {
                await _apiClient!.GetTodosAsync();
                Console.WriteLine("✓ Connected to server successfully!");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to connect to server: {ex.Message}");
                Console.WriteLine("Please make sure the server is running.");
                Environment.Exit(1);
            }
        }

        private static async Task ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("1. List todos (l)");
                Console.WriteLine("2. Add todo (a)");
                Console.WriteLine("3. Toggle todo status (t)");
                Console.WriteLine("4. Quit (q)");
                Console.Write("Enter command: ");

                var input = Console.ReadLine()?.Trim().ToLower();

                try
                {
                    switch (input)
                    {
                        case "1":
                        case "l":
                        case "list":
                            await ListTodos();
                            break;
                        case "2":
                        case "a":
                        case "add":
                            await AddTodo();
                            break;
                        case "3":
                        case "t":
                        case "toggle":
                            await ToggleTodo();
                            break;
                        case "4":
                        case "q":
                        case "quit":
                        case "exit":
                            Console.WriteLine("Goodbye!");
                            return;
                        default:
                            Console.WriteLine("Invalid command. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }

        private static async Task ListTodos()
        {
            var todos = await _apiClient!.GetTodosAsync();

            if (todos.Count == 0)
            {
                Console.WriteLine("No todos found.");
                return;
            }

            Console.WriteLine("Todo List:");
            Console.WriteLine("---------");
            foreach (var todo in todos)
            {
                var status = todo.IsCompleted ? "[✓]" : "[ ]";
                Console.WriteLine($"{status} {todo.Id}: {todo.Description}");
            }
        }

        private static async Task AddTodo()
        {
            Console.Write("Enter todo description: ");
            var description = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(description))
            {
                Console.WriteLine("Description cannot be empty.");
                return;
            }

            var newTodo = await _apiClient!.AddTodoAsync(description);
            Console.WriteLine($"Added todo: {newTodo.Id} - {newTodo.Description}");
        }

        private static async Task ToggleTodo()
        {
            Console.Write("Enter todo ID to toggle: ");
            var input = Console.ReadLine()?.Trim();

            if (!int.TryParse(input, out int todoId))
            {
                Console.WriteLine("Invalid ID. Please enter a number.");
                return;
            }

            try
            {
                await _apiClient!.UpdateTodoStatusAsync(todoId);
                Console.WriteLine($"Toggled status for todo {todoId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to toggle todo: {ex.Message}");
            }
        }
    }
}