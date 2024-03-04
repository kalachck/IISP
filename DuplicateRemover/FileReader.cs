using BenchmarkDotNet.Attributes;

namespace DuplicateRemover;

[MemoryDiagnoser(false)]
public class FileReader
{
    private static readonly string _path = @"C:\Work Files\unik\IISP\PasswordCracker\DuplicateRemover\passwords.txt";
    
    [Benchmark]
    public async Task<long> GetLinesCountAsync_ReadLinesAsync()
    {
        var linesCount = 0;

        await foreach (var _ in File.ReadLinesAsync(_path))
        {
            linesCount++;
        }

        return linesCount;
    }

    [Benchmark]
    public async Task<long> GetLinesCountAsync_ReadAllLinesAsync()
    {
        return (await File.ReadAllLinesAsync(_path)).Length;
    }
    
    [Benchmark]
    public long GetFileInfoLength()
    {
        return new FileInfo(_path).Length;
    }
}
