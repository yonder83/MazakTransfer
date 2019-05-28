using MazakTransfer.Database;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using MazakTransfer.Util;

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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DrawingService.CreateDatabaseIfNotExists();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception exception = null;
            if (e.Exception != null)
            {
                exception = e.Exception.InnerException ?? e.Exception;
            }

            if (exception == null) return;


            if (!(exception is MazakException))
            {
                try
                {
                    //Log to file if it is not MazakException
                    Logger.Error(exception.Message);
                }
                catch
                {
                }
            }

            if (MainWindow is MainWindow mainWindow)
            {
                //Show error in UI
                mainWindow.WriteLogLine(exception.Message, StatusLevel.Error);

                //MainWindow is initialized and showing, we need to set exception Handled
                e.Handled = true;
            }
        }
    }
}
