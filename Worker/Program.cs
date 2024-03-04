using Grpc.Core;
using Passwordcracker;

namespace Worker;

class Program
{
    private static string _host = "localhost";
    private static int _port = 5051;
    
    public static async Task Main(string[] args)
    {
        Dictionary<string, string?> parameters = ParseCommandLineArguments(args);
        
        if (parameters.TryGetValue("host", out var hostValue))
        {
            _host = hostValue!;
        }
        else
        {
            Console.WriteLine($"Хост не был указан или указан неверно, используется хост по умолчанию: {_host}");
        }

        if (parameters.ContainsKey("port") && int.TryParse(parameters["port"], out var portValue))
        {
            _port = portValue;
        }
        else
        {
            Console.WriteLine($"Порт не был указан или указан неверно, используется хост по умолчанию: {_port}");
        }

        var startServer = Task.Run(() =>
        {
            var server = new Server
            {
                Services = { PasswordCracker.BindService(new PasswordCrackerService()) },
                Ports = { new ServerPort(_host, _port, ServerCredentials.Insecure) }
            };
            server.Start();
            
            Console.ReadKey();
            server.ShutdownAsync().Wait();
        });

        Console.WriteLine($"Worker запущен по адресу http://{_host}:{_port}");
        Console.WriteLine("Нажмите любую клавишу для выхода...");

        await startServer;
        
        Console.WriteLine("Завершение работы воркера...");
    }
    
    private static Dictionary<string, string?> ParseCommandLineArguments(string[] args)
    {
        var parameters = new Dictionary<string, string?>();
        foreach (var arg in args)
        {
            string?[] parts = arg.Split('=');
            if (parts.Length == 2)
            {
                parameters[parts[0].TrimStart('-').ToLower()] = parts[1];
            }
        }
        return parameters;
    }
}
