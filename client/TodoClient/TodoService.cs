using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace TodoClient
{
    public class TodoService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private bool _disposed;

        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler? TodoListUpdated;

        public TodoService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<List<TodoItem>> GetTodosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/todos");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var todoResponses = JsonConvert.DeserializeObject<List<TodoResponse>>(json) ?? new List<TodoResponse>();

                var todos = todoResponses.Select(tr => new TodoItem
                {
                    Id = tr.Id,
                    Description = tr.Description,
                    IsCompleted = tr.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                }).ToList();

                ConnectionStatusChanged?.Invoke(this, true);
                return todos;
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(this, false);
                throw new Exception($"Failed to get todos from server: {ex.Message}", ex);
            }
        }

        public async Task<TodoItem> AddTodoAsync(string description)
        {
            try
            {
                var request = new AddTodoRequest { Description = description };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/todos", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var todoResponse = JsonConvert.DeserializeObject<TodoResponse>(responseJson);

                if (todoResponse == null)
                    throw new Exception("Invalid response from server");

                var todo = new TodoItem
                {
                    Id = todoResponse.Id,
                    Description = todoResponse.Description,
                    IsCompleted = todoResponse.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                };

                ConnectionStatusChanged?.Invoke(this, true);
                TodoListUpdated?.Invoke(this, EventArgs.Empty);
                
                return todo;
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(this, false);
                throw new Exception($"Failed to add todo: {ex.Message}", ex);
            }
        }

        public async Task UpdateTodoStatusAsync(int todoId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/todos/{todoId}", null);
                response.EnsureSuccessStatusCode();

                ConnectionStatusChanged?.Invoke(this, true);
                TodoListUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(this, false);
                throw new Exception($"Failed to update todo status: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}