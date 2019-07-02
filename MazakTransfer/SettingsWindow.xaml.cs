using System.Windows;
using System.Windows.Forms;

namespace MazakTransfer
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    { 
        public SettingsWindow()
        {
            InitializeComponent();

            TextBoxLocalPath.Text = Properties.Settings.Default.LocalPath;
            TextBoxMazakPath.Text = Properties.Settings.Default.MazakPath;
            TextBoxUpdateInterval.Text = Properties.Settings.Default.FileListUpdateInterval.ToString();
        }

        private void ButtonLocalPath_Click(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.LocalPath))
                {
                    folderBrowserDialog.SelectedPath = Properties.Settings.Default.LocalPath;
                }

                folderBrowserDialog.ShowDialog();
                TextBoxLocalPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonMazakPath_Click(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.MazakPath))
                {
                    folderBrowserDialog.SelectedPath = Properties.Settings.Default.MazakPath;
                }

                folderBrowserDialog.ShowDialog();
                TextBoxMazakPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            //TODO: save settings to custom settings file
            //Properties.Settings.Default.LocalPath = TextBoxLocalPath.Text;
            //Properties.Settings.Default.MazakPath = TextBoxMazakPath.Text;
            //Properties.Settings.Default.Save();
            Close();
        }
    }
}
