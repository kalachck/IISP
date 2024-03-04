using Grpc.Core;
using Passwordcracker;

namespace Worker;

public class PasswordCrackerService : PasswordCracker.PasswordCrackerBase
{
    private static readonly string _filePath = "passwords.txt";
    private string _secretPassword = string.Empty;
    
    private bool CheckPassword(string candidate)
    {
        return candidate == _secretPassword;
    }

    public override async Task CrackPassword(
        IAsyncStreamReader<CrackRequest> requestStream,
        IServerStreamWriter<CrackResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                Console.WriteLine($"Начинаем взлом пароля для воркера {request.WorkerIndex} из {request.WorkersCount}");

                _secretPassword = request.Password;

                if (TryCrackPassword(_filePath, request.WorkerIndex, request.WorkersCount, out var foundPassword))
                {
                    await responseStream.WriteAsync(new CrackResponse
                    {
                        Success = true,
                        FoundPassword = foundPassword,
                        WorkerAddress = request.WorkerAddress
                    });
                    break;
                }

                await responseStream.WriteAsync(new CrackResponse
                {
                    Success = false,
                    WorkerAddress = request.WorkerAddress
                });
            }
        }
        catch (OperationCanceledException)
        {
            await responseStream.WriteAsync(new CrackResponse
            {
                Success = false,
            });
        }
    }
    
    public bool TryCrackPassword(string filePath, int workerIndex, int totalWorkers, out string foundPassword)
    {
        foundPassword = string.Empty;

        var a = new FileInfo(filePath).Length;
        
        long fileSize;
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fileSize = fileStream.Length;
        }

        long partSize = fileSize / totalWorkers;
        long startOffset = partSize * workerIndex;
        long endOffset = (workerIndex == totalWorkers - 1) ? fileSize : startOffset + partSize;

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new StreamReader(fileStream))
            {
                if (workerIndex > 0)
                {
                    fileStream.Seek(startOffset, SeekOrigin.Begin);
                    while (reader.Peek() >= 0 && reader.Read() != '\n') continue;
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (fileStream.Position > endOffset) break;

                    if (!string.IsNullOrWhiteSpace(line) && CheckPassword(line))
                    {
                        Console.WriteLine($"Password found: {line}");
                        
                        foundPassword = line;
                        return true;
                    }
                }

                Console.WriteLine("Password not found");
                
                return false;
            }
        }

    }
}
