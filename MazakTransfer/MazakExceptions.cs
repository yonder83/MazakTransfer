using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazakTransfer
{
    public class MazakException : Exception
    {
        private readonly StatusLevel _level;

        public MazakException(string message, StatusLevel level) : base(message)
        {
            _level = level;
        }

        public StatusLevel Level
        {
            get { return _level; }
        }
    }
}
