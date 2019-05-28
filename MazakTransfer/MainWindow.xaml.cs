using MazakTransfer.Database;
using MazakTransfer.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MazakTransfer
{
    public enum StatusLevel
    {
        Normal,
        Error,
        Completed
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DrawingService _drawingService;

        public IEnumerable<FileData> FileDatasMachine { get; set; }
        public IEnumerable<FileData> FileDatasRepository { get; set; }

        private const string LayoutFileExtension = ".lay";
        private const string SetupFileExtension = ".stp";
        private const string MazFileExtension = ".maz";

        private const string LayoutPath = "Layout";
        private const string SetUpPath = "Set Up";

        public MainWindow()
        {           
            //Initialisoi mm. Filelistit
            InitializeComponent();

            buttonSaveComment.Content = Properties.Resources.ButtonTextSaveComment;
            labelVersion.Content = Properties.Resources.TextVersion + Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            if (CheckApplicationPaths() == false)
                return;

            InitializeFileLists();
            textBoxNumber.Focus();

            _drawingService = new DrawingService();
            _drawingService.CreateDatabaseIfNotExists();
        }

        private bool CheckApplicationPaths()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.MazakPath))
            {
                WriteLogLine("Määritä työstökoneen kansio.", StatusLevel.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPath))
            {
                WriteLogLine("Määritä paikallinen kansio.", StatusLevel.Error);
                return false;
            }

            return true;
        }

        private void InitializeFileLists()
        {
            //Annetaan tiedostolistoille polut
            listViewRepositoryFiles.FilePath = Properties.Settings.Default.LocalPath;
            listViewMachineFiles.FilePath = Properties.Settings.Default.MazakPath;

            //Kaapataan valintamuutoseventit
            listViewRepositoryFiles.FileSelectionChanged += ListViewFileSelectionChanged;
            listViewMachineFiles.FileSelectionChanged += ListViewFileSelectionChanged;
        }

        //Kun tiedostolistan valinta muuttuu, hae myös oikea tekstikommentti.
        void ListViewFileSelectionChanged(object sender, EventArgs e)
        {
            bool machineFileSelected = listViewMachineFiles.SelectedItem != null;
            bool repositoryFileSelected = listViewRepositoryFiles.SelectedItem != null;
            bool isMachineListView = Equals(sender, listViewMachineFiles);
            bool updateComment = false;

            //Ollaan valittu molemmat, tilanne että vaihdetaan näkymää
            if (machineFileSelected && repositoryFileSelected)
            {
                //Näytetään valitun kommentti
                updateComment = true;

                //Kun vaihdetaan näkymien välillä, pyyhitään toisen näkymän valinta
                if (isMachineListView)
                {
                    listViewRepositoryFiles.SelectedItem = null;
                }
                else
                {
                    listViewMachineFiles.SelectedItem = null;
                }
            }
            else if (machineFileSelected || repositoryFileSelected)
            {
                //Vaihdetaan valintaa saman näkymän sisällä, tai valitaan se ensimmäistä kertaa. 
                //Tänne tullaa myös kun valinta katoaa näkymästä, sen jälkeen kun toinen on valittu, silloin ei päivitetä kommenttia
                if ((isMachineListView && machineFileSelected) || (!isMachineListView && repositoryFileSelected))
                {
                    updateComment = true;
                }
            }
            else
            {
                //Kummassakaan ei ole valittu mitään. Tämä tilanne tulee esim siirron ja poiston jälkeen. Siivotaan kommentti
                updateComment = true;
            }

            if (updateComment)
            {
                string drawingNumber = GetSelectedDrawingNumber(!isMachineListView);
                ShowDrawingComment(drawingNumber);
            }
        }

        private void buttonToMazak_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoTransfer(true);
            }
            catch (MazakException ex)
            {
                WriteLogLine(ex.Message, ex.Level);
            }
        }

        private void buttonFromMazak_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoTransfer(false);
            }
            catch (MazakException ex)
            {
                WriteLogLine(ex.Message, ex.Level);
            }
        }

        //Kirjoittaa rivin statusbaariin
        public void WriteLogLine(string line, StatusLevel level = StatusLevel.Normal)
        {
            var run = new Run(line + Environment.NewLine);
            switch (level)
            {
                case StatusLevel.Normal:
                    break;
                case StatusLevel.Completed:
                    run.Foreground = Brushes.Green;
                    break;
                case StatusLevel.Error:
                    run.Foreground = Brushes.Red;
                    break;

            }

            //statusText voi olla alussa myös null!
            if (statusText != null)
            {
                statusText.Inlines.Clear();
                statusText.Inlines.Add(run);
            }

            //Käynnistetään storyboard joka animoi statustekstin
            if (Resources["StatusTextFadeOut"] is Storyboard storyboard)
            {
                storyboard.Begin();
            }
        }

        /// <summary>
        /// Palauttaa valitun piirustuksen numeron. 
        /// </summary>
        /// <param name="fromRepositoryList">Jos true, palautetaan varastoslistalta. Jos false, palautetaan työstökoneen listalta. Jos null, palautetaan kummasta vaan</param>
        /// <returns></returns>
        private string GetSelectedDrawingNumber(bool? fromRepositoryList = null)
        {
            FileData selectedItem;
            if (fromRepositoryList.HasValue)
            {
                selectedItem = fromRepositoryList.Value ? listViewRepositoryFiles.SelectedItem : listViewMachineFiles.SelectedItem;
            }
            else
            {
                selectedItem = listViewRepositoryFiles.SelectedItem ?? listViewMachineFiles.SelectedItem;
            }

            string drawingNumber = selectedItem != null ? selectedItem.FileNameWithoutExtension : String.Empty;

            return drawingNumber;
        }

        private void DoTransfer(bool toMazak)
        {
            //get paths
            string localPath = Properties.Settings.Default.LocalPath;
            string mazakPath = Properties.Settings.Default.MazakPath;

            //Get drawing number
            string drawingNumber = GetSelectedDrawingNumber(toMazak);

            try
            {
                //check drawing number. If something is wrong, exit
                CheckDrawingNumber(drawingNumber);
            }
            catch (MazakException ex)
            {
                WriteLogLine(ex.Message, ex.Level);
                return;
            }

            string localMazFile = Path.Combine(localPath, drawingNumber + MazFileExtension);
            string localLayoutFile = Path.Combine(localPath, LayoutPath, drawingNumber + LayoutFileExtension);
            string localSetUpFile = Path.Combine(localPath, SetUpPath, drawingNumber + SetupFileExtension);
            string mazakMazFile = Path.Combine(mazakPath, drawingNumber + MazFileExtension);
            string mazakLayoutFile = Path.Combine(mazakPath, LayoutPath, drawingNumber + LayoutFileExtension);
            string mazakSetUpFile = Path.Combine(mazakPath, SetUpPath, drawingNumber + SetupFileExtension);

            if (toMazak)
            {
                if (!CheckFiles(localMazFile, localLayoutFile, localSetUpFile))
                    return;

                if (!AskConfirmationIfFilesExist(mazakMazFile, mazakLayoutFile, mazakSetUpFile))
                    return;

                try
                {
                    File.Copy(localMazFile, mazakMazFile, true);
                    File.Copy(localLayoutFile, mazakLayoutFile, true);
                    File.Copy(localSetUpFile, mazakSetUpFile, true);
                }
                catch (Exception ex)
                {
                    WriteLogLine(ex.Message, StatusLevel.Error);
                    return;
                }
            }
            else
            {
                if (!CheckFiles(mazakMazFile, mazakLayoutFile, mazakSetUpFile))
                    return;

                if (!AskConfirmationIfFilesExist(localMazFile, localLayoutFile, localSetUpFile))
                    return;

                try
                {
                    File.Copy(mazakMazFile, localMazFile, true);
                    File.Copy(mazakLayoutFile, localLayoutFile, true);
                    File.Copy(mazakSetUpFile, localSetUpFile, true);

                    //Delete from mazak
                    if (checkBoxDeleteAfterTransfer.IsChecked.GetValueOrDefault())
                    {
                        IO.DeleteFiles(mazakMazFile, mazakLayoutFile, mazakSetUpFile);
                    }
                }
                catch (Exception ex)
                {
                    WriteLogLine(ex.Message, StatusLevel.Error);
                    return;
                }
            }

            //Refresh filelists. These are asyncronous operations
            listViewMachineFiles.Refresh();
            listViewRepositoryFiles.Refresh();

            WriteLogLine("Siirto Onnistui", StatusLevel.Completed);
        }

        private bool CheckFiles(params string[] files)
        {
            bool ret = true;

            
            string fileNames = String.Empty;
            foreach (string file in files.Where(file => !File.Exists(file)))
            {
                if (fileNames != String.Empty)
                {
                    fileNames += ", ";
                }

                fileNames += file;
                ret = false;
            }

            string statusBarMessage = String.Format("Tiedostoja ei löydy: {0}", fileNames);

            if (ret == false)
            {
                WriteLogLine(statusBarMessage, StatusLevel.Error);
            }

            return ret;
        }

        private bool AskConfirmationIfFilesExist(params string[] files)
        {
            var filesExist = files.Where(File.Exists).ToList();

            if (filesExist.Count != 0)
            {
                // Configure the message box to be displayed
                string messageBoxText = filesExist.Aggregate("Korvataanko tiedostot", (current, file) => current + ("\n" + file));
                string caption = "Piirustus on jo olemassa";
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                if (result == MessageBoxResult.OK)
                    return true;

                return false;
            }

            return true;
        }

        private bool AskConfirmationForFileDelete(params string[] files)
        {
            var filesExist = files.Where(File.Exists).ToList();

            if (filesExist.Count != 0)
            {
                // Configure the message box to be displayed
                string messageBoxText = filesExist.Aggregate(Properties.Resources.ConfirmationForFileDelete, (current, file) => current + ("\n" + file));
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                MessageBoxResult result = MessageBox.Show(messageBoxText, Properties.Resources.ConfirmationCaption, button, icon);

                if (result == MessageBoxResult.OK)
                    return true;

                return false;
            }

            return true;
        }

        private void textBoxNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Sallitaan vain numerot
            bool isNumber = Regex.IsMatch(e.Text, Constants.NUMBERS_PATTERN);
            e.Handled = !isNumber;
        }

        private void ShowDrawingComment(string fileName)
        {
            //Siivoa kommenttikenttä aluksi
            richTextBoxComment.Document.Blocks.Clear();

            if (fileName != null)
            {
                var comment = _drawingService.GetDrawingCommentByName(fileName);

                if (!string.IsNullOrEmpty(comment))
                {
                    TextRange textRange = new TextRange(richTextBoxComment.Document.ContentStart, richTextBoxComment.Document.ContentEnd);

                    byte[] byteArray = Encoding.UTF8.GetBytes(comment);
                    using (MemoryStream stream = new MemoryStream(byteArray))
                    {
                        // Detect in which format text is. Earlier versions of MazakTransfer used Rtf format to save comments.
                        bool isRtf = comment.StartsWith(@"{\rtf");
                        textRange.Load(stream, isRtf ? DataFormats.Rtf : DataFormats.Text);
                    }
                }
            }
        }

        //Event handleri tekstiboksin tekstimuutoksille.
        private void textBoxNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Rajataan tiedostolistoja
            listViewRepositoryFiles.FilterString = textBoxNumber.Text;
            listViewMachineFiles.FilterString = textBoxNumber.Text;
        }

        //Save comment to DB
        private void buttonSaveComment_Click(object sender, RoutedEventArgs e)
        {
            var drawingNumber = GetSelectedDrawingNumber();
            CheckDrawingNumber(drawingNumber);

            TextRange textRange = new TextRange(richTextBoxComment.Document.ContentStart, richTextBoxComment.Document.ContentEnd);
            string text = textRange.Text;

            bool saved = _drawingService.SaveCommentByDrawingName(drawingNumber, text);
            if (saved)
            {
                WriteLogLine(String.Format(Properties.Resources.SuccessTextCommentSaved, drawingNumber), StatusLevel.Completed);
            }
        }

        private void buttonRemoveDrawingFromMachine_Click(object sender, RoutedEventArgs e)
        {
            string mazakPath = Properties.Settings.Default.MazakPath;

            //Get machines drawing number
            string drawingNumber = GetSelectedDrawingNumber(false);

            try
            {
                //Validoidaan, jos tiedostoa ei ole valittu, poistutaan.
                CheckDrawingNumber(drawingNumber);
            }
            catch (MazakException ex)
            {
                WriteLogLine(ex.Message, ex.Level);
                return;
            }

            string mazakMazFile = Path.Combine(mazakPath, drawingNumber + MazFileExtension);
            string mazakLayoutFile = Path.Combine(mazakPath, LayoutPath, drawingNumber + LayoutFileExtension);
            string mazakSetUpFile = Path.Combine(mazakPath, SetUpPath, drawingNumber + SetupFileExtension);

            //Tarkistetaan että tiedostot ovat olemassa
            if (!CheckFiles(mazakMazFile, mazakLayoutFile, mazakSetUpFile))
                return;

            //Kysytään vahvistus poistosta
            if (!AskConfirmationForFileDelete(mazakMazFile, mazakLayoutFile, mazakSetUpFile))
                return;

            try
            {
                IO.DeleteFiles(mazakMazFile, mazakLayoutFile, mazakSetUpFile);
            }
            catch (Exception ex)
            {
                WriteLogLine(ex.Message, StatusLevel.Error);
                return;
            }

            //Refresh filelist. this is asyncronous operations
            listViewMachineFiles.Refresh();

            WriteLogLine(Properties.Resources.SuccessMachineFileDelete, StatusLevel.Completed);
        }

        public static void CheckDrawingNumber(string drawingNumber)
        {
            if (String.IsNullOrWhiteSpace(drawingNumber))
            {
                throw new MazakException("Valitse piirustusnumero", StatusLevel.Error);
            }
            // check even if user cannot input other than numbers. Value can be copy pasted
            if (!Regex.IsMatch(drawingNumber, Constants.NUMBERS_PATTERN))
            {
                throw new MazakException("Piirustusnumero voi sisältää vain numeroita", StatusLevel.Error);
            }
        }
    }
}
