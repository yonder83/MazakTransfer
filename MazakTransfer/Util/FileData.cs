using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace MazakTransfer.Util
{
    public class FileData : INotifyPropertyChanged 
    {
        public FileAttributes FileAttributes { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string FileNameWithoutExtension { get; set; }
        private string _fileName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName
        {
            get { return _fileName; }
            private set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private DateTime FiletimeToDatetime(FILETIME fileTime)
        {
            long highBits = fileTime.dwHighDateTime;
            highBits = highBits << 32;

            return DateTime.FromFileTimeUtc(highBits + (uint)fileTime.dwLowDateTime);
        }

        //Loput kentät on varalle
        public FileData(FileAttributes dwFileAttributes,
            FILETIME ftCreationTime,
            FILETIME ftLastAccessTime,
            FILETIME ftLastWriteTime,
            int nFileSizeHigh,
            int nFileSizeLow,
            int dwReserved0,
            int dwReserved1,
            string cFileName,
            string cAlternate)
        {
            FileName = cFileName;
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(cFileName);
            LastWriteTime = FiletimeToDatetime(ftLastWriteTime);
        }
    }
}
