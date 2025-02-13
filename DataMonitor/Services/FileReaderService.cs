using System.IO;

namespace DataMonitor.Services
{
    public class FileReaderService : IFileReaderService
    {
        public async Task<string> ReadFileAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
