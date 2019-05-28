namespace MazakTransfer.Util
{
    //Contains some Win32 error codes.
    //https://docs.microsoft.com/en-us/windows/desktop/Debug/system-error-codes

    public static class SystemErrorCodes
    {
        public static uint ERROR_SUCCESS = 0x0;
        public static uint ERROR_INVALID_FUNCTION = 0x1;
        public static uint ERROR_FILE_NOT_FOUND = 0x2;
        public static uint ERROR_PATH_NOT_FOUND = 0x3;
        public static uint ERROR_TOO_MANY_OPEN_FILES = 0x4;
        public static uint ERROR_ACCESS_DENIED = 0x5;
        public static uint ERROR_INVALID_HANDLE = 0x6;
        public static uint ERROR_ARENA_TRASHED = 0x7;
        public static uint ERROR_NOT_ENOUGH_MEMORY = 0x8;
        public static uint ERROR_INVALID_BLOCK = 0x9;
        public static uint ERROR_BAD_ENVIRONMENT = 0xA;
        public static uint ERROR_BAD_NETPATH = 0x35;
        public static uint ERROR_BAD_NET_NAME = 0x43;

    }
}