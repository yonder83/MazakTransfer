using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace MazakTransfer.Util
{
    public class FileSearcher
    {
        private readonly Dispatcher _dispatcher;
        private Action<IEnumerable<FileData>> _doneAction;
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
                    _doneAction = actionToCallWhenDone;
                    Func<string, string, IEnumerable<FileData>> methodToCallAsync = DirectoryManager.GetFilesInfo;
                    methodToCallAsync.BeginInvoke(filePath, searchPattern, SearchDone, methodToCallAsync);
                }
            }
        }

        private void SearchDone(IAsyncResult ar)
        {
            _searching = false;
            var target = (Func<string, string, IEnumerable<FileData>>) ar.AsyncState;
            IEnumerable<FileData> result = target.EndInvoke(ar);

            //Kutsutaan paluumetodia, palautetaan sille tiedostolista
            _dispatcher.Invoke(_doneAction, result);
        }
    }
}
