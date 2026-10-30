using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace xRAR.Views
{
    public partial class ExtractDialog : Window
    {
        public string DestinationPath { get; private set; } = "";

        public ExtractDialog(string archivePath, List<string>? selectedFiles)
        {
            InitializeComponent();

            // Default destination: same folder as archive
            var dir = Path.GetDirectoryName(archivePath) ?? "";
            var name = Path.GetFileNameWithoutExtension(archivePath);
            DestPath.Text = Path.Combine(dir, name);
            DestPath.Items.Add(DestPath.Text);
            DestPath.Items.Add(dir);
            DestPath.Items.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), name));

            if (selectedFiles != null && selectedFiles.Count > 0)
            {
                SelectedInfo.Text = $"{selectedFiles.Count} file(s) selected from {Path.GetFileName(archivePath)}:\n" +
                    string.Join("\n", selectedFiles.Take(10)) +
                    (selectedFiles.Count > 10 ? $"\n... and {selectedFiles.Count - 10} more" : "");
            }
            else
            {
                SelectedInfo.Text = $"All files from {Path.GetFileName(archivePath)}";
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WinForms.FolderBrowserDialog
            {
                SelectedPath = DestPath.Text,
                Description = "Select destination folder"
            };
            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
                DestPath.Text = dlg.SelectedPath;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DestPath.Text))
            {
                MessageBox.Show("Please enter a destination path.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dest = DestPath.Text.Trim();
            if (ChkExtractToSubfolder.IsChecked == true)
            {
                // Already includes archive name subfolder by default
            }

            try
            {
                Directory.CreateDirectory(dest);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot create directory: {ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DestinationPath = dest;
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
