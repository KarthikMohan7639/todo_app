using Grpc.Net.Client;
using TodoApp.Grpc;

namespace TodoClient
{
    public class TodoService
    {
        private readonly GrpcChannel _channel;
        private readonly TodoApp.Grpc.TodoService.TodoServiceClient _client;

        public TodoService()
        {
            _channel = GrpcChannel.ForAddress("http://localhost:50051");
            _client = new TodoApp.Grpc.TodoService.TodoServiceClient(_channel);
        }

        public async Task<List<TodoItem>> GetTodosAsync()
        {
            try
            {
                var request = new GetTodosRequest();
                var response = await _client.GetTodosAsync(request);

                return response.TodoItems.Select(ConvertFromGrpc).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get todos: {ex.Message}", ex);
            }
        }

        public async Task<TodoItem> AddTodoAsync(string description)
        {
            try
            {
                var request = new AddTodoRequest { Description = description };
                var response = await _client.AddTodoAsync(request);

                if (!response.Success)
                {
                    throw new Exception(response.Message);
                }

                return ConvertFromGrpc(response.TodoItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add todo: {ex.Message}", ex);
            }
        }

        public async Task<TodoItem> UpdateTodoStatusAsync(int id)
        {
            try
            {
                var request = new UpdateTodoStatusRequest { Id = id };
                var response = await _client.UpdateTodoStatusAsync(request);

                if (!response.Success)
                {
                    throw new Exception(response.Message);
                }

                return ConvertFromGrpc(response.TodoItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update todo: {ex.Message}", ex);
            }
        }

        private TodoItem ConvertFromGrpc(TodoApp.Grpc.TodoItem grpcItem)
        {
            return new TodoItem
            {
                Id = grpcItem.Id,
                Description = grpcItem.Description,
                IsCompleted = grpcItem.Status == TodoApp.Grpc.TodoStatus.Completed
            };
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}