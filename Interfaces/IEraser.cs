namespace FilesBackuperWpf.Interfaces
{
    public interface IEraser
    {
        void Erase(string folderPath);
        Task EraseAsync(string folderPath);
        void SetMaxFolders(int count);
    }
}
