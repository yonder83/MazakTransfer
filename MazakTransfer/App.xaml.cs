using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace MazakTransfer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Set current culture to FI, TODO put that configurable
            var newCulture = new CultureInfo("fi-FI");
            Thread.CurrentThread.CurrentCulture = newCulture;

            var xmlLanguage = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);
            var languageMetaData = new FrameworkPropertyMetadata(xmlLanguage);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), languageMetaData);

            //Set DataDirectory. EF connection string uses that
            AppDomain.CurrentDomain.SetData("DataDirectory", MazakTransfer.Properties.Settings.Default.DatabasePath);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var mainWindow =  MainWindow as MainWindow;
            if (mainWindow != null) 
                mainWindow.WriteLogLine(e.Exception.Message, StatusLevel.Error);
            
            e.Handled = true;
        }
    }
}
