using MazakTransfer.Database;
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
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (MainWindow is MainWindow mainWindow)
            {
                Exception exception = null;
                if (e.Exception != null )
                {
                    exception = e.Exception.InnerException ?? e.Exception;
                }
            
                if (exception != null)
                {
                    mainWindow.WriteLogLine(exception.Message, StatusLevel.Error);
                }
            }

            e.Handled = true;
        }
    }
}
