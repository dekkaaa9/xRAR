using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace xRAR.Views
{
    public partial class AddDialog : Window
    {
        private readonly string _currentPath;
        private readonly string? _archiveContext;
        public List<string> SelectedFiles { get; private set; } = new();
        public string ArchivePath => ArchiveName.Text.Trim();
        public string Format => RbZip.IsChecked == true ? "zip" : "rar";
        public int CompressionLevel => CmbCompressionLevel.SelectedIndex;
        public string? Password => string.IsNullOrEmpty(PwdBox.Password) ? null : PwdBox.Password;

        public AddDialog(string currentPath, string? archiveContext)
        {
            InitializeComponent();
            _currentPath = currentPath;
            _archiveContext = archiveContext;

            // Default archive name
            var defaultName = Path.Combine(currentPath, "archive.rar");
            ArchiveName.Text = defaultName;
            ArchiveName.Items.Add(defaultName);
            ArchiveName.Items.Add(Path.Combine(currentPath, "archive.zip"));
        }

        private void BrowseArchive_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Archive name and location",
                Filter = "RAR archive|*.rar|ZIP archive|*.zip|All files|*.*",
                FileName = ArchiveName.Text,
                InitialDirectory = _currentPath
            };
            if (dlg.ShowDialog() == true)
            {
                ArchiveName.Text = dlg.FileName;
                RbRar.IsChecked = dlg.FileName.EndsWith(".rar", StringComparison.OrdinalIgnoreCase);
                RbZip.IsChecked = dlg.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select files to add",
                Multiselect = true,
                InitialDirectory = _currentPath
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedFiles.AddRange(dlg.FileNames);
                FilesInfo.Text = $"{SelectedFiles.Count} file(s):\n" +
                    string.Join("\n", SelectedFiles.ConvertAll(f => Path.GetFileName(f)).Take(8)) +
                    (SelectedFiles.Count > 8 ? $"\n... and {SelectedFiles.Count - 8} more" : "");
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArchivePath))
            {
                MessageBox.Show("Please enter an archive name.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedFiles.Count == 0)
            {
                MessageBox.Show("Please add at least one file.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
