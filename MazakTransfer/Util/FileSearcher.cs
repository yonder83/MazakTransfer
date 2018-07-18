using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MazakTransfer.Util
{
    public class FileSearcher
    {
        private readonly Dispatcher _dispatcher;
        private bool _searching;

        public FileSearcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void SearchFileList(string filePath, string searchPattern, Action<IEnumerable<FileData>> actionToCallWhenDone)
        {
            //Tehdään asynkronisesti tiedostojen haku
            if (!String.IsNullOrWhiteSpace(filePath) && !String.IsNullOrWhiteSpace(searchPattern))
            {
                //Jos toinen haku on päällä, ei tehdä uutta hakua
                if (!_searching)
                {
                    _searching = true;

                    var tokenSource = new CancellationTokenSource();

                    var task = Task.Factory.StartNew(SearchBackground(filePath, searchPattern), tokenSource.Token);
                    var continuationTask = task.ContinueWith(t =>
                    {
                        _searching = false;

                        //Kutsutaan paluumetodia, palautetaan sille tiedostolista
                        _dispatcher.Invoke(actionToCallWhenDone, t.Result);
                    }, tokenSource.Token);
                }
            }
        }

        private static Func<List<FileData>> SearchBackground(string filePath, string searchPattern)
        {
            //Create file searcher thread. GetFilesInfo returns with yield, so we must  make List here to get all files at once to continuation thread
            return () => DirectoryManager.GetFilesInfo(filePath, searchPattern).ToList();
        }
    }
}
