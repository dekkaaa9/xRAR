using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace xRAR.Views
{
    public partial class InfoDialog : Window
    {
        public InfoDialog(string path, Dictionary<string, string>? archiveInfo = null)
        {
            InitializeComponent();

            var items = new List<KeyValuePair<string, string>>();

            if (archiveInfo != null)
            {
                TitleText.Text = "Archive Information";
                foreach (var kv in archiveInfo)
                    items.Add(kv);
            }
            else if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                TitleText.Text = di.Name;
                items.Add(new("Name", di.Name));
                items.Add(new("Type", "File folder"));
                items.Add(new("Path", di.FullName));
                items.Add(new("Created", di.CreationTime.ToString("dd/MM/yyyy HH:mm:ss")));
                items.Add(new("Modified", di.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss")));
                items.Add(new("Accessed", di.LastAccessTime.ToString("dd/MM/yyyy HH:mm:ss")));
                try { items.Add(new("Contains", $"{di.GetFiles().Length} files, {di.GetDirectories().Length} folders")); } catch { }
            }
            else if (File.Exists(path))
            {
                var fi = new FileInfo(path);
                TitleText.Text = fi.Name;
                items.Add(new("Name", fi.Name));
                items.Add(new("Type", Models.FileSystemItem.GetFileType(fi.Extension)));
                items.Add(new("Size", $"{Models.FileSystemItem.FormatBytesExact(fi.Length)} bytes"));
                items.Add(new("Path", fi.DirectoryName ?? ""));
                items.Add(new("Created", fi.CreationTime.ToString("dd/MM/yyyy HH:mm:ss")));
                items.Add(new("Modified", fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss")));
                items.Add(new("Accessed", fi.LastAccessTime.ToString("dd/MM/yyyy HH:mm:ss")));
                items.Add(new("Attributes", Models.FileSystemItem.GetAttrString(fi.Attributes)));
            }

            InfoList.ItemsSource = items;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
