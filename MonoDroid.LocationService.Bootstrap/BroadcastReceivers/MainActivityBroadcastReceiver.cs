using System;
using Android.Content;
using Android.App;

namespace MonoDroid.LocationService.Bootstrap.BroadcastReceivers
{
    [BroadcastReceiver]
    [IntentFilter(new string[]{AppConstants.APPLICATION_COMMAND}, Categories = new string[]{Intent.CategoryDefault})]
    public class MainActivityBroadcastReceiver : BroadcastReceiver
    {
        public event Action<Context, Intent> Receive;

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent != null && intent.Action == AppConstants.APPLICATION_COMMAND)
            {
                if (this.Receive != null)
                {
                    this.Receive(context, intent);
                }
            }
        }
    }
}