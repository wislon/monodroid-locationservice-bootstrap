namespace MonoDroid.LocationService.Bootstrap
{
    public static class AppConstants
    {
        public const string COMMAND_TYPE_ID = "COMMAND_TYPE_ID";
        public const string SERVICE_COMMAND = "SERVICE_COMMAND";

        public enum ServiceCommandType
        {
            Unknown = -1,
            SendPing = 0,
            StopService = 1,
        }

        public const string APPLICATION_COMMAND = "APPLICATION_COMMAND";

        public enum ApplicationCommandType
        {
            Unknown = -1,
            ReceivePong = 0,
        }

    }


}