using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MazakTransfer
{
    class Validation
    {
        public static void CheckLocalFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new MazakException("Määritä paikallinen kansio.", StatusLevel.Error);
            }

            if (!Directory.Exists(path))
            {
                throw new MazakException("Paikallista kansiota " + path + " ei löydy.", StatusLevel.Error);
            }
        }

        public static void CheckMazakFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new MazakException("Määritä työstökoneen kansio.", StatusLevel.Error);
            }

            if (!Directory.Exists(path))
            {
                throw new MazakException("Työstökoneen kansiota " + path + " ei löydy.", StatusLevel.Error);
            }
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
