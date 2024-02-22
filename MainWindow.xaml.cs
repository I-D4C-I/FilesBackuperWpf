using FilesBackuperWpf.Classes;
using FilesBackuperWpf.Utility;
using Ookii.Dialogs.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace FilesBackuperWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            Backuper.Notify += WriteInLog;
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            BtnStopBackup.IsEnabled = false;
        }

        private async void _timer_Tick(object? sender, EventArgs e)
        {
            await Backuper.StartBackupAsync();
        }

        private void WriteInLog(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TBlockLog.Text += "\n" + message;
            }));
        }

        public string OpenDirectoryDialog()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog();

            if (result == true)
                return dialog.SelectedPath;
            else
                return string.Empty;
        }

        private void BtnSelectSourceDir_Click(object sender, RoutedEventArgs e)
        {
            var sorceDir = OpenDirectoryDialog();
            if (string.IsNullOrEmpty(sorceDir))
                return;
            TBSorceDir.Text = sorceDir;
            Backuper.SourceDir = sorceDir;
        }

        private void BtnSelectDestDir_Click(object sender, RoutedEventArgs e)
        {
            var destDir = OpenDirectoryDialog();
            if (string.IsNullOrEmpty(destDir))
                return;
            TBDestDir.Text = destDir;
            Backuper.DestinationDir = destDir;
            Backuper.SetLogger(new FileLogger(destDir, Constants.BACKUP_LOG_FILE));
        }

        private async void BtnStartBackup_Click(object sender, RoutedEventArgs e)
        {
            BtnStartBackup.IsEnabled = false;

            //Backuper.StartBackup();
            await Backuper.StartBackupAsync();

            if (int.TryParse(tb_BackupsCount.Text, out var backupCount))
            {
                var eraser = new FolderEraser();
                eraser.SetMaxFolders(backupCount);
                Backuper.SetEraser(eraser);
            }

            if (int.TryParse(TBTimeSpan.Text, out var timeSpan))
            {
                BtnStopBackup.IsEnabled = true;
                _timer.Interval = TimeSpan.FromMinutes(timeSpan);
                _timer.Start();
            }
            else
            {
                BtnStartBackup.IsEnabled = true;
            }
        }
        private void BtnStopBackup_Click(object sender, RoutedEventArgs e)
        {
            BtnStopBackup.IsEnabled = false;
            _timer.Stop();
            BtnStartBackup.IsEnabled = true;
        }
    }
}