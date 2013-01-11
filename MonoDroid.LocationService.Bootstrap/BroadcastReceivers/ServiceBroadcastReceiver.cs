using System;
using Android.Content;

namespace MonoDroid.LocationService.Bootstrap.BroadcastReceivers
{
    public class ServiceBroadcastReceiver : BroadcastReceiver
    {
        public Action<Context, Intent> ReceiveMessage;

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent != null && intent.Action == ApplicationConstants.SERVICE_COMMAND)
            {
                if (ReceiveMessage != null)
                {
                    ReceiveMessage(context, intent);
                }
            }
        }
    }
}