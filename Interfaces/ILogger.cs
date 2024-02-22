namespace FilesBackuperWpf.Interfaces
{
    public interface ILogger
    {
        Task LogAsync(string msg);

        void Log(string msg);
    }
}
