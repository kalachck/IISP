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
            Console.WriteLine($"The host was not specified or specified incorrectly, the default host is used: {_host}");
        }

        if (parameters.ContainsKey("port") && int.TryParse(parameters["port"], out var portValue))
        {
            _port = portValue;
        }
        else
        {
            Console.WriteLine($"The port was not specified or specified incorrectly, the default host is used: {_port}");
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

        Console.WriteLine($"Worker is launched at http://{_host}:{_port}");
        Console.WriteLine("Press any key to exit...");

        await startServer;
        
        Console.WriteLine("Completion of the worker's work...");
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
