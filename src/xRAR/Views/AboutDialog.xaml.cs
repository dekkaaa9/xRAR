using System.Windows;

namespace xRAR.Views
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
