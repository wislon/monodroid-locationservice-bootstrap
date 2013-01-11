using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using MonoDroid.LocationService.Bootstrap.BroadcastReceivers;

namespace MonoDroid.LocationService.Bootstrap.Services
{
    [Service(Label = "Test Service")]
    public class TestService : Service
    {
        private readonly ServiceBroadcastReceiver _sbr;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public TestService()
        {
            _sbr = new ServiceBroadcastReceiver();
            _sbr.ReceiveMessage += (context, intent) =>
                                       {
                                           var cType = (ApplicationConstants.ServiceCommandType) intent.GetIntExtra(ApplicationConstants.COMMAND_TYPE_ID, -1);
                                           switch (cType)
                                           {
                                               case ApplicationConstants.ServiceCommandType.SendPing:
                                                   {
                                                       Log.Info("TestService", "Ping received");
                                                       break;
                                                   }
                                               case ApplicationConstants.ServiceCommandType.StopService:
                                                   {
                                                       Log.Info("TestService", "Service stopping...");
                                                       StopSelf();
                                                       break;
                                                   }
                                               default:
                                                   {
                                                       Log.Info("TestService", "Unknown Command: {0}", cType.ToString());
                                                       break;
                                                   }
                                           }
                                       };
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("TestService.OnStartCommand", "started");
            var startCommandResult = base.OnStartCommand(intent, flags, startId);

            RegisterReceiver(_sbr, new IntentFilter(ApplicationConstants.SERVICE_COMMAND));

            return startCommandResult;
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(_sbr);
            Log.Info("TestService.OnDestroy", "Finished");
        }

    }
}