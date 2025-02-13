namespace DataMonitor.Services
{
    public interface IFileReaderService
    {
        Task<string> ReadFileAsync(string filePath);
    }
}
