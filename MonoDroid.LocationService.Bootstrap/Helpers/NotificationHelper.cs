using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace MonoDroid.LocationService.Bootstrap.Helpers
{
    public class NotificationHelper
    {
        public static void SendBroadcastForToastMessage(Context context, string messageToToast)
        {
            var toastIntent = new Intent(AppConstants.APPLICATION_COMMAND);
            toastIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int)AppConstants.ApplicationCommandType.ShowToastMessage);
            toastIntent.PutExtra(AppConstants.TOAST_MESSAGE_KEY, messageToToast);
            context.SendBroadcast(toastIntent);
        }


    }
}