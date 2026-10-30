using System.Windows;

namespace xRAR.Views
{
    public partial class WizardDialog : Window
    {
        public WizardDialog()
        {
            InitializeComponent();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (RbExtract.IsChecked == true)
            {
                MessageBox.Show("Please open an archive first, then use Extract To.", "xRAR Wizard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (RbCreate.IsChecked == true)
            {
                MessageBox.Show("Please use the Add button to create a new archive.", "xRAR Wizard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (RbConvert.IsChecked == true)
            {
                MessageBox.Show("SFX conversion requires Default.SFX module.\nCommand line: Rar sfx archive.rar", "xRAR Wizard", MessageBoxButton.OK, MessageBoxImage.Information);
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
