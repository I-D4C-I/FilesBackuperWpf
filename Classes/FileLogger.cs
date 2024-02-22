using FilesBackuperWpf.Interfaces;
using System.IO;
using System.Text;

namespace FilesBackuperWpf.Classes
{
    class FileLogger : ILogger
    {
        private readonly string _logFilePath = default!;

        public FileLogger(string destination, string fileName)
        {
            _logFilePath = destination + "/" + fileName;
        }

        public void Log(string msg)
        {
            using var stream = new StreamWriter(_logFilePath, true, Encoding.Default);
            stream.WriteLine(msg);
        }

        public async Task LogAsync(string msg)
        {
            using var stream = new StreamWriter(_logFilePath, true, Encoding.Default);
            await stream.WriteLineAsync(msg);
        }
    }
}
