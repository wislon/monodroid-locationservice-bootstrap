namespace MonoDroid.LocationService.Bootstrap
{
    public static class AppConstants
    {
        public const string COMMAND_TYPE_ID = "COMMAND_TYPE_ID";
        public const string SERVICE_COMMAND = "SERVICE_COMMAND";

        public const string EXPORTED_FILE_NAME = "EXPORTED_FILE_NAME";

        public enum ServiceCommandType
        {
            Unknown = -1,
            SendPing = 0,
            StopService = 1,
            ExportData
        }

        public const string APPLICATION_COMMAND = "APPLICATION_COMMAND";

        public enum ApplicationCommandType
        {
            Unknown = -1,
            ReceivePong = 0,
            DataExported
        }

    }


}