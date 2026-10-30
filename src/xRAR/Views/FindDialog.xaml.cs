using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using xRAR.Models;

namespace xRAR.Views
{
    public partial class FindDialog : Window
    {
        private readonly string _startPath;
        private readonly ObservableCollection<FileSystemItem> _results = new();

        public FindDialog(string startPath)
        {
            InitializeComponent();
            _startPath = startPath;
            ResultsList.ItemsSource = _results;
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            _results.Clear();
            var pattern = SearchPattern.Text.Trim();
            if (string.IsNullOrEmpty(pattern)) return;

            try
            {
                var searchOption = ChkRecursive.IsChecked == true
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                foreach (var file in Directory.GetFiles(_startPath, pattern, searchOption))
                {
                    var fi = new FileInfo(file);
                    _results.Add(FileSystemItem.FromFileInfo(fi));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "xRAR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
