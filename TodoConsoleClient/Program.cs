using Grpc.Net.Client;
using TodoApp.Grpc;

namespace TodoConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string serverUrl = args.Length > 0 ? args[0] : "http://localhost:50051";
            
            Console.WriteLine("Todo List Console Client (gRPC)");
            Console.WriteLine("===============================");
            Console.WriteLine($"Connecting to server: {serverUrl}");
            Console.WriteLine();

            using var channel = GrpcChannel.ForAddress(serverUrl);
            var client = new TodoService.TodoServiceClient(channel);

            while (true)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1. List all todos");
                Console.WriteLine("2. Add a new todo");
                Console.WriteLine("3. Toggle todo status");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice (1-4): ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await ListTodosAsync(client);
                            break;
                        case "2":
                            await AddTodoAsync(client);
                            break;
                        case "3":
                            await ToggleTodoStatusAsync(client);
                            break;
                        case "4":
                            Console.WriteLine("Goodbye!");
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static async Task ListTodosAsync(TodoService.TodoServiceClient client)
        {
            Console.WriteLine("\nRetrieving todos...");
            
            var request = new GetTodosRequest();
            var response = await client.GetTodosAsync(request);

            if (response.Success)
            {
                Console.WriteLine($"\nTodo List ({response.TodoItems.Count} items):");
                Console.WriteLine("ID | Status    | Description");
                Console.WriteLine("---|-----------|------------");
                
                foreach (var todo in response.TodoItems)
                {
                    var status = todo.Status == TodoStatus.Completed ? "Completed" : "Pending";
                    Console.WriteLine($"{todo.Id,2} | {status,-9} | {todo.Description}");
                }
            }
            else
            {
                Console.WriteLine($"Failed to retrieve todos: {response.Message}");
            }
        }

        static async Task AddTodoAsync(TodoService.TodoServiceClient client)
        {
            Console.Write("\nEnter todo description: ");
            var description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Description cannot be empty.");
                return;
            }

            Console.WriteLine("Adding todo...");
            
            var request = new AddTodoRequest { Description = description };
            var response = await client.AddTodoAsync(request);

            if (response.Success)
            {
                Console.WriteLine($"Todo added successfully! ID: {response.TodoItem.Id}");
            }
            else
            {
                Console.WriteLine($"Failed to add todo: {response.Message}");
            }
        }

        static async Task ToggleTodoStatusAsync(TodoService.TodoServiceClient client)
        {
            Console.Write("\nEnter todo ID to toggle: ");
            var idInput = Console.ReadLine();

            if (!int.TryParse(idInput, out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a number.");
                return;
            }

            Console.WriteLine("Updating todo status...");
            
            var request = new UpdateTodoStatusRequest { Id = id };
            var response = await client.UpdateTodoStatusAsync(request);

            if (response.Success)
            {
                var status = response.TodoItem.Status == TodoStatus.Completed ? "Completed" : "Pending";
                Console.WriteLine($"Todo {id} status updated to: {status}");
            }
            else
            {
                Console.WriteLine($"Failed to update todo: {response.Message}");
            }
        }
    }
}