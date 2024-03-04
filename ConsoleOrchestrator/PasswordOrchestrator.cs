using Grpc.Core;
using Grpc.Net.Client;
using Passwordcracker;

namespace ConsoleOrchestrator;

public class PasswordOrchestrator
{
    private readonly List<string> _workerAddresses;
    private readonly string _realPassword;

    public PasswordOrchestrator(
        List<string> workerAddresses,
        string realPassword)
    {
        _workerAddresses = workerAddresses;
        _realPassword = realPassword;
    }
    
    public async Task DistributeAndCrackPasswordAsync()
    {
        foreach (var workerAddress in _workerAddresses)
        {
            await InitiateCrackingSession(workerAddress);
        }
    }

    private async Task InitiateCrackingSession(string workerAddress)
    {
        using var channel = GrpcChannel.ForAddress(workerAddress);
        var client = new PasswordCracker.PasswordCrackerClient(channel);

        using var call = client.CrackPassword();

        var requestStream = call.RequestStream;
        var responseStream = call.ResponseStream;

        await requestStream.WriteAsync(new CrackRequest
        {
            WorkerIndex = _workerAddresses.IndexOf(workerAddress),
            WorkersCount = _workerAddresses.Count,
            Password = _realPassword,
            WorkerAddress = workerAddress
        });

        await requestStream.CompleteAsync();

        await foreach (var response in responseStream.ReadAllAsync())
        {
            if (response.Success)
            {
                Console.WriteLine($"Password was found \"{response.FoundPassword}\" by worker {response.WorkerAddress}");
                break;
            }
            
            Console.WriteLine($"Password was not found by {response.WorkerAddress} ");
        }
    }
    
    // public async Task DistributeAndCrackPasswordAsync()
    // {
    //     var totalWorkers = _workerAddresses.Count;
    //         
    //     var tasks = new List<Task<CrackResponse>>();
    //
    //     for (var i = 0; i < totalWorkers; i++)
    //     {
    //         var workerIndex = i;
    //         var workerAddress = _workerAddresses[i];
    //         tasks.Add(Task.Run(() => CrackPasswordAsync(workerAddress, workerIndex, totalWorkers)));
    //     }
    //
    //     CrackResponse result = await await Task.WhenAny(tasks);
    //
    //     if (!result.Success)
    //     {
    //         Console.WriteLine("Password not found.");
    //         return;
    //     }
    //     
    //     Console.WriteLine($"Password found {result.FoundPassword}, by worker with address: {result.WorkerAddress}");
    // }
    //
    // private async Task<CrackResponse> CrackPasswordAsync(
    //     string workerAddress,
    //     int workerIndex,
    //     int totalWorkers)
    // {
    //     using var channel = GrpcChannel.ForAddress(workerAddress);
    //     var client = new PasswordCracker.PasswordCrackerClient(channel);
    //
    //     var request = new CrackRequest
    //     {
    //         WorkerIndex = workerIndex,
    //         WorkersCount = totalWorkers,
    //         Password = _realPassword,
    //         WorkerAddress = workerAddress
    //     };
    //
    //     try
    //     {
    //         return await client.CrackPasswordAsync(request);
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error during password cracking: {ex.Message}");
    //         return new CrackResponse { Success = false };
    //     }
    // }
}
