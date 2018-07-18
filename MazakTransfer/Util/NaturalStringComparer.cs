using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace MazakTransfer.Util
{
    public sealed class NaturalStringComparer : IComparer
    {
        private string propertyName;
        ListSortDirection direction;

        public NaturalStringComparer(string sortPropertyName, ListSortDirection sortDirection)
        {
            propertyName = sortPropertyName;
            direction = sortDirection;
        }

        public int Compare(object x, object y)
        {
            var a = x as FileData;
            var b = y as FileData;

            int result;
            if (direction == ListSortDirection.Ascending)
            {
                result = SafeNativeMethods.StrCmpLogicalW(a.FileName, b.FileName);
            }
            else
            {
                result = SafeNativeMethods.StrCmpLogicalW(b.FileName, a.FileName);
            }

            return result;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }
}
