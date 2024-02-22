using FilesBackuperWpf.Interfaces;
using System.IO;

namespace FilesBackuperWpf.Classes
{
    public class FolderEraser : IEraser
    {
        private int _maxFolders;
        public void SetMaxFolders(int count)
        {
            _maxFolders = count;
        }

        private IEnumerable<DirectoryInfo>? GetDirToDelete(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            if (subDirectories.Length < _maxFolders)
                return null;

            var sortedDirectories = subDirectories.OrderBy(d => d.LastWriteTime);
            return sortedDirectories.Take(subDirectories.Length - _maxFolders);
        }

        public void Erase(string folderPath)
        {
            if(_maxFolders < 0) return;

            var directoriesToDelete = GetDirToDelete(folderPath);
            if (directoriesToDelete == null) return;

            foreach (var directory in directoriesToDelete)
                directory.Delete(true);
        }

        public async Task EraseAsync(string folderPath)
        {
            if (_maxFolders < 0) return;

            var directoriesToDelete = GetDirToDelete(folderPath);
            if (directoriesToDelete == null) return;

            foreach (var directory in directoriesToDelete)
                await Task.Run(() => directory.Delete(true));
        }

    }
}
