namespace ConsoleOrchestrator;

class Program
{
    private static List<string> _workersAddresses = new ();
    private static string _password = string.Empty;
    
    static async Task Main(string[] args)
    {
        var parameters = ParseCommandLineArguments(args);
            
        if (parameters.TryGetValue("addresses", out string? addresses))
        {
            _workersAddresses = addresses.Split(',').ToList();
        }

        if (parameters.TryGetValue("password", out string? password))
        {
            _password = password;
        }

        ValidateParameters();

        try
        {
            var orchestrator = new PasswordOrchestrator(_workersAddresses, _password);

            Console.WriteLine("Распределение задач между воркерами...");

            var startTime = DateTime.Now;
            
            await orchestrator.DistributeAndCrackPasswordAsync();

            Console.WriteLine($"Программа завершена за {(DateTime.Now - startTime).TotalSeconds}");
            
            Console.ReadKey();
        }
        catch (Grpc.Core.RpcException e)
        {
            Console.WriteLine($"Ошибка gRPC: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Произошла ошибка: {e.Message}");
        }
        finally
        {
            Console.WriteLine($"Завершение работы оркестратора.");
        }
    }
    
    private static Dictionary<string, string> ParseCommandLineArguments(string[] args)
    {
        var parameters = new Dictionary<string, string>();
        foreach (var arg in args)
        {
            var parts = arg.Split('=');
            if (parts.Length == 2)
            {
                parameters[parts[0].TrimStart('-').ToLower()] = parts[1];
            }
        }
        return parameters;
    }

    private static void ValidateParameters()
    {
        if (_workersAddresses.Count == 0)
        {
            Console.WriteLine("Не указаны адреса воркеров. Завершение работы оркестратора.");
            Environment.Exit(0);
        }
        
        if (string.IsNullOrWhiteSpace(_password))
        {
            Console.WriteLine("Не указан пароль. Завершение работы оркестратора.");
            Environment.Exit(0);
        }
    }
}
