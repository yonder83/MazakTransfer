using log4net;
using log4net.Config;

namespace MazakTransfer.Util
{
    public static class Logger
    {
        static Logger()
        {
            XmlConfigurator.Configure();
        }

        public static ILog ProgramLogger => LogManager.GetLogger("ProgramLogger");
    }
}
