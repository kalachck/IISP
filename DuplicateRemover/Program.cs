using System.Text;
using BenchmarkDotNet.Running;

namespace DuplicateRemover;

class Program
{
    private static readonly string _path = @"C:\\Work Files\\unik\\IISP\\PasswordCracker\\passwords.txt";

    static void Main(string[] args)
    {
        Console.SetCursorPosition(10, 5);
        Console.Write("Это новая позиция курсора");


    }

    private static string GetAddSomethingCool()
    {
        return new string("asdasd");
    }
    
    private static void RemoveDuplicates()
    {
        var distinctData = File.ReadAllLines(_path).Distinct().ToList();
        
        File.WriteAllLines(_path, distinctData);
    }

    private static string ReadFromPosition()
    {
        var fileLength = new FileInfo(_path).Length;
        
        using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
        {
            fs.Seek(0, SeekOrigin.Begin);

            var buffer = new byte[(int)Math.Round(fileLength / 2.0)];

            fs.Read(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
