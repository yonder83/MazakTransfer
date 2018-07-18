using System.IO;

namespace MazakTransfer.Util
{
    public class IO
    {
        public static void DeleteFiles(params string[] files)
        {
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
