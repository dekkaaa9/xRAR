using System;
using System.Windows;
using System.Windows.Controls;
using xRAR.Services;

namespace xRAR.Views
{
    public partial class SettingsDialog : Window
    {
        private static readonly string[] Extensions = new[]
        {
            ".rar", ".zip", ".zipx", ".7z", ".tar", ".gz", ".bz2", ".xz",
            ".cab", ".arj", ".lzh", ".iso", ".zst"
        };

        public SettingsDialog()
        {
            InitializeComponent();
            LoadAssociations();
        }

        private void LoadAssociations()
        {
            AssocPanel.Children.Clear();
            foreach (var ext in Extensions)
            {
                bool registered = FileAssociationService.IsRegistered(ext);
                var cb = new CheckBox
                {
                    Content = ext,
                    IsChecked = registered,
                    Margin = new Thickness(0, 2, 12, 2),
                    Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary"),
                    Tag = ext
                };
                cb.Checked += AssocCheckBox_Changed;
                cb.Unchecked += AssocCheckBox_Changed;
                AssocPanel.Children.Add(cb);
            }
        }

        private void AssocCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is string ext)
            {
                try
                {
                    if (cb.IsChecked == true)
                        FileAssociationService.RegisterExtension(ext);
                    else
                        FileAssociationService.UnregisterExtension(ext);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update association for {ext}:\n{ex.Message}\n\nTry running xRAR as Administrator.",
                        "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cb.IsChecked = !cb.IsChecked;
                }
            }
        }

        private void BtnRegisterAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = FileAssociationService.RegisterAll();
                MessageBox.Show($"Registered {result.Count} file associations.\n\nDouble-clicking archive files will now open xRAR.",
                    "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAssociations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register:\n{ex.Message}\n\nTry running xRAR as Administrator.",
                    "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnUnregisterAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileAssociationService.UnregisterAll();
                MessageBox.Show("All file associations removed.", "xRAR", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAssociations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to unregister:\n{ex.Message}", "xRAR", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
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
