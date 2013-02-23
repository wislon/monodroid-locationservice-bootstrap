namespace MonoDroid.LocationService.Bootstrap
{
    public static class AppConstants
    {

        public const string COMMAND_TYPE_ID = "COMMAND_TYPE_ID";
        public const string EXPORTED_FILE_NAME = "EXPORTED_FILE_NAME";

        public const string SERVICE_COMMAND = "SERVICE_COMMAND";

        public enum ServiceCommandType
        {
            Unknown = -1,
            SendPing = 0,
            StopService = 1,
            ExportData,
            UploadData
        }

        public const string APPLICATION_COMMAND = "APPLICATION_COMMAND";

        public enum ApplicationCommandType
        {
            Unknown = -1,
            ReceivePong = 0,
            ShowToastMessage,
            DataExported,
            DataUploading,
            DataUploaded,
        }

        public const string UPLOAD_COMMAND = "UPLOAD_COMMAND";

        public enum UploadCommandType
        {
            Unknown = -1,
            StartUpload = 0,
            CancelUpload
        }

        public const string TOAST_MESSAGE_KEY = "TOAST_MESSAGE_KEY";


    }


}