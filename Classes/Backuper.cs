using FilesBackuperWpf.Interfaces;
using FilesBackuperWpf.Utility;
using System.IO;

namespace FilesBackuperWpf.Classes
{

    public delegate void ResultHandler(string msg);

    public static class Backuper
    {
        private static string _sourceDir = default!;
        private static string _sourceDirName = default!;
        private static string _destinationDir = default!;
        private static string _backupDir = default!;
        private static DateTime _startDate;
        private static ILogger? _logger;
        private static IEraser? _eraser;

        public static event ResultHandler? Notify;
        public static string SourceDir
        {
            get
            {
                return _sourceDir;
            }
            set
            {
                _sourceDir = value;
                _sourceDirName = value[value.LastIndexOf('\\')..];
            }
        }
        public static string DestinationDir
        {
            get
            {
                return _destinationDir;
            }
            set
            {
                _startDate = DateTime.Now;
                _destinationDir = value;
            }
        }

        public static void SetLogger(ILogger logger) { _logger = logger; }
        public static void SetEraser(IEraser eraser) { _eraser = eraser; }

        private static bool IsDirectoryCorrect()
        {
            if (IsPathsEmpty())
            {
                Log("Ошибка установленных путей");
                return false;
            }

            if (!Directory.Exists(SourceDir))
            {
                Log("Исходная папка отсутсвует");
                return false;
            }

            return true;
        }
        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
        }

        private static bool IsPathsEmpty()
        {
            if (string.IsNullOrEmpty(_sourceDir) || string.IsNullOrEmpty(_destinationDir))
                return true;
            return false;
        }

        private static bool IsNextDay(DateTime currentDate, DateTime compareDate)
        {
            TimeSpan difference = currentDate.Date - compareDate.Date;
            return Math.Abs(difference.Days) >= 1;
        }

        private static void EraseBackups()
        {
            if (_eraser == null)
                return;
            _eraser.Erase(_destinationDir + _sourceDirName + "_backup");
            Log("Очистка старых Backup");
        }

        private static async Task EraseBackupsAsync()
        {
            if (_eraser == null)
                return;
            await Task.Run(() => _eraser.EraseAsync(_destinationDir + _sourceDirName + "_backup"));
            await LogAsync("Очистка старых Backup");
        }

        public static void StartBackup()
        {
            WriteSeparator();
            Log("Начат Backup");
            CreateBackup();
            Log("Backup закончен");
            WriteSeparator();
        }

        public static async Task StartBackupAsync()
        {
            WriteSeparator();
            Log("Начат Backup");
            await CreateBackupAsync();
            Log("Backup закончен");
            WriteSeparator();
        }

        private static void CreateBackup()
        {

            if (!IsDirectoryCorrect())
                return;

            Log($"Исходная папка: {_sourceDir}");

            if (IsNextDay(DateTime.Now, _startDate))
            {
                _startDate = DateTime.Now;
                EraseBackups();
            }

            _backupDir = GenerateBackupDirPath(_destinationDir);

            CreateDirIfNotExist(_backupDir);

            Log($"Целевая папка: {_destinationDir}");

            Log($"Backup {_sourceDirName}");
            CopyDirectory(_sourceDir, _backupDir);
        }

        private static async Task CreateBackupAsync()
        {

            if (!IsDirectoryCorrect())
                return;

            await LogAsync($"Исходная папка: {_sourceDir}");

            if (IsNextDay(DateTime.Now, _startDate))
            {
                _startDate = DateTime.Now;
                EraseBackupsAsync();
            }

            _backupDir = GenerateBackupDirPath(_destinationDir);

            CreateDirIfNotExist(_backupDir);

            await LogAsync($"Целевая папка: {_destinationDir}");

            await LogAsync($"Backup {_sourceDirName}");
            await Task.Run(() => CopyDirectoryAsync(_sourceDir, _backupDir));

        }

        public static void CreateDirIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Log("Целевая папка отсутсвует");
                Directory.CreateDirectory(path);
                Log("Целевая папка создана");
            }
        }

        public static void CopyDirectory(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            foreach (string file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    string relativePath = file[(sourceFolder.Length + 1)..];
                    string destFile = Path.Combine(destFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, true);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log("Exception:\n" + ex.Message);
                }
            }
        }

        public static async Task CopyDirectoryAsync(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            await Task.Run(() =>
            {
                foreach (string file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        string relativePath = file[(sourceFolder.Length + 1)..];
                        string destFile = Path.Combine(destFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        File.Copy(file, destFile, true);
                    }
                    catch (Exception ex)
                    {
                        Log("Exception:\n" + ex.Message);
                    }
                }
            });
        }

        public static string GenerateBackupDirPath(string destinationPath)
        {
            if (string.IsNullOrEmpty(_sourceDir))
                return destinationPath + $"\\backup\\{_startDate.DayOfWeek}-{_startDate.Day}-{_startDate.Month}";
            var sourceFolder = _sourceDir[_sourceDir.LastIndexOf('\\')..];
            return destinationPath + $"\\{sourceFolder}_backup\\{_startDate.DayOfWeek}-{_startDate.Day}-{_startDate.Month}";
        }

        public static void Log(string msg)
        {
            Notify?.Invoke($"{GetCurrentTime()}: {msg}");
            _logger?.Log($"{GetCurrentTime()}: {msg}");
        }

        public static async Task LogAsync(string msg)
        {
            await Task.Run(() =>
            {
                Notify?.Invoke($"{GetCurrentTime()}: {msg}");
                _logger?.LogAsync($"{GetCurrentTime()}: {msg}");
            });
        }

        private static void WriteSeparator()
        {
            Notify?.Invoke(Constants.SEPATATOR);
            _logger?.Log(Constants.SEPATATOR);
        }

    }
}
