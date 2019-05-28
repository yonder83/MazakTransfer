using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace MazakTransfer.Util
{
    internal static class DirectoryManager
    {
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        private static string MakePath(string path, string searchPattern)
        {
            if (!path.EndsWith("\\"))
                path += "\\";
            return Path.Combine(path, searchPattern);
        }

        private static IEnumerable<Win32FindData> GetInternalFileInfo(string path, string searchPattern, bool isGetDirs)
        {
            Win32FindData findData;

            IntPtr findHandle = FindFirstFile(MakePath(path, searchPattern), out findData);

            if (findHandle == InvalidHandleValue)
            { 
                //Check error
                uint error = GetLastError();
                //TODO: log error to file

                //If error is path not found, throw exception
                if (error == SystemErrorCodes.ERROR_PATH_NOT_FOUND || error == SystemErrorCodes.ERROR_BAD_NETPATH)
                {
                    throw new MazakException("Kansiota " + path + " ei löydy.", StatusLevel.Error);
                }

                yield break;
            }
            try
            {
                do
                {
                    if (findData.cFileName == "." || findData.cFileName == "..")
                        continue;
                    if (isGetDirs ? (findData.dwFileAttributes & FileAttributes.Directory) != 0 : (findData.dwFileAttributes & FileAttributes.Directory) == 0)
                    {
                        yield return findData;
                    }
                } while (FindNextFile(findHandle, out findData));
            }
            finally
            {
                FindClose(findHandle);
            }
        }

        private static IEnumerable<string> GetInternal(string path, string searchPattern, bool isGetDirs)
        {
            return GetInternalFileInfo(path, searchPattern, isGetDirs).Select(win32FindData => Path.Combine(path, win32FindData.cFileName));
        }

        public static IEnumerable<FileData> GetFilesInfo(string path, string searchPattern)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (searchPattern == null) throw new ArgumentNullException("searchPattern");
            
            return GetInternalFileInfo(path, searchPattern, false).Select(w => new FileData(w.dwFileAttributes, w.ftCreationTime, w.ftLastAccessTime, w.ftLastWriteTime, 
                w.nFileSizeHigh, w.nFileSizeLow, w.dwReserved0, w.dwReserved1, w.cFileName, w.cAlternate));
        }

        public static IEnumerable<string> GetFiles(string path, string searchPattern)
        {
            return GetInternal(path, searchPattern, false);
        }

        public static IEnumerable<string> GetDirectories(string path, string searchPattern)
        {
            return GetInternal(path, searchPattern, true);
        }

        public static IEnumerable<string> GetAllDirectories(string path)
        {
            foreach (string subDir in GetDirectories(path, "*"))
            {
                if (subDir == ".." || subDir == ".")
                    continue;
                string relativePath = Path.Combine(path, subDir);
                
                yield return relativePath;
                
                foreach (string subDir2 in GetAllDirectories(relativePath))
                {
                    yield return subDir2;
                }
            }
        }

        #region Import from kernel32

        private const int MAX_PATH = 260;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [BestFitMapping(false)]
        private struct Win32FindData
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternate;
        }

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string lpFileName, out Win32FindData lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr hFindFile, out Win32FindData lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        #endregion
    }
}
