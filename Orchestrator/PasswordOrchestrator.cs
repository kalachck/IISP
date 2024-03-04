using System.Windows.Controls;
using Grpc.Net.Client;
using Passwordcracker;
using Grpc.Core;

namespace Orchestrator;

public class PasswordOrchestrator
{
    private readonly List<string> _workerAddresses;
    private readonly string _password;

    public PasswordOrchestrator(
        List<string> workerAddresses,
        string password)
    {
        _workerAddresses = workerAddresses;
        _password = password;
    }
    
    public async Task DistributeAndCrackPasswordAsync(TextBox textBlock)
    {
        foreach (var workerAddress in _workerAddresses)
        {
            await InitiateCrackingSession(workerAddress, textBlock);
        }
    }

    private async Task InitiateCrackingSession(string workerAddress, TextBox textBlock)
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
            Password = _password,
            WorkerAddress = workerAddress
        });

        await requestStream.CompleteAsync();

        await foreach (var response in responseStream.ReadAllAsync())
        {
            if (response.Success)
            {
                textBlock.Text += $"Password was found \"{response.FoundPassword}\" by worker {response.WorkerAddress}\n";
                
                break;
            }
            
            textBlock.Text += $"Password was not found by {response.WorkerAddress}\n";
        }
    }
}
