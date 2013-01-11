using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.IO;
using MonoDroid.LocationService.Bootstrap.BroadcastReceivers;

namespace MonoDroid.LocationService.Bootstrap.Services
{
    [Service(Label = "Location Monitoring")]
    public class LocationMonitoringService : Service, ILocationListener
    {

        private NotificationManager _notificationManager;
        private string _bestProvider;
        private LocationManager _locationManager;

        private ServiceBroadcastReceiver _sbr;


        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }


        public override void OnCreate()
        {
            Log.Info("LMService.OnCreate", string.Format("In OnCreate"));
            base.OnCreate();

            _sbr = new ServiceBroadcastReceiver();
            _sbr.ReceiveMessage += (context, intent) =>
                                       {
                                           var cType = (AppConstants.ServiceCommandType) intent.GetIntExtra(AppConstants.COMMAND_TYPE_ID, -1);
                                           switch (cType)
                                           {
                                               case AppConstants.ServiceCommandType.SendPing:
                                                   {
                                                       Log.Info("TestService", "Ping received");
                                                       var pongIntent = new Intent(AppConstants.APPLICATION_COMMAND);
                                                       pongIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int) AppConstants.ApplicationCommandType.ReceivePong);
                                                       Log.Info("TestService", "Sending pong!");
                                                       SendBroadcast(pongIntent);
                                                       break;
                                                   }
                                               case AppConstants.ServiceCommandType.StopService:
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

            _notificationManager = (NotificationManager) GetSystemService(Context.NotificationService);
            ShowNotification();
            var criteria = new Criteria
                               {
                                   Accuracy = Accuracy.Fine,
                                   PowerRequirement = Power.High,
                                   AltitudeRequired = false,
                                   BearingRequired = false,
                                   SpeedRequired = false,
                                   CostAllowed = false,
                               };
            _locationManager = (LocationManager) GetSystemService(Context.LocationService);
            _bestProvider = _locationManager.GetBestProvider(criteria, false);
            Location _location = _locationManager.GetLastKnownLocation(_bestProvider);
            // at least 15 seconds between updates, at least 100 metres between updates, 'this' because it implements ILocationListener.
            _locationManager.RequestLocationUpdates(_bestProvider, 15000, 100, this);

        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("LMService.OnStartCommand", "In OnStartCommand");
            base.OnStartCommand(intent, flags, startId);

            Log.Info("LMService.OnStartCommand", "Received start ID {0} : {1}", startId, intent);
            Log.Info("LMService.OnStartCommand", "Registering broadcast receivers...");
            RegisterReceiver(_sbr, new IntentFilter(AppConstants.SERVICE_COMMAND)); // so we can listen for commands

            // so we don't close if/when the app does. We have to stop it specifically.
            return StartCommandResult.Sticky; 
        }


        public override void OnDestroy()
        {
            Log.Info("LMService.OnDestroy", "In OnDestroy");
            base.OnDestroy();
            _notificationManager.Cancel(Resource.String.LocationMonitoringServiceStarted);
            Toast.MakeText(this, Resource.String.LocationMonitoringServiceStopped, ToastLength.Short).Show();
            _locationManager.RemoveUpdates(this);
            UnregisterReceiver(_sbr);
            StopSelf();
        }

        private void ShowNotification()
        {
            var text = GetText(Resource.String.LocationMonitoringServiceStarted);
            TriggerStandardNotification(text);
        }

        private void TriggerStandardNotification(string text)
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var now = (long) sinceEpoch.TotalMilliseconds;

            var notification = new Notification(Resource.Drawable.Icon, text, now);

            // This iss setting up an intent that will be attached to the notification, so when you click the 
            // notification in the notification bar, it'll kick off 'LMSActivity' (our 'launcher')
            PendingIntent contentIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof (LMSActivity)), 0);
            notification.SetLatestEventInfo(this,
                                            GetText(Resource.String.LocationMonitoringServiceLabel),
                                            text,
                                            contentIntent);

            // the 'R.S.LocationMonitoringServiceStarted' is an ID unique to this app's resources and is used as a key 
            // to reference this notification message in future (like if we want to cancel it automatically).
            _notificationManager.Notify(Resource.String.LocationMonitoringServiceStarted, notification);
        }

        public void OnLocationChanged(Location location)
        {
            var text = string.Format("Lat: {0} Lon: {1}", location.Latitude, location.Longitude);
            TriggerStandardNotification(text); // this can get annoying after a bit, but it's useful to see if the service is still running.
            WriteLocationFileEntry(location);
        }

        private void WriteLocationFileEntry(Location location)
        {
            string eol = System.Environment.NewLine;

            string locationInfo = string.Format("{0}\t{1}\t{2}{3}", location.Latitude, location.Longitude, DateTime.UtcNow.ToString("s"), eol);

            // this will get a directory named /data/data/<app name>/app_lmsData/. 
            // You'll only be able to get to this file on the device itself if you have root access.
            var path = GetDir("lmsData", FileCreationMode.Private);
            var outputFile = new File(path, "locationData.txt");

            var encoding = new UTF8Encoding();
            using (var fos = new FileOutputStream(outputFile, true))
            {
                fos.Write(encoding.GetBytes(locationInfo));
                fos.Flush();
                fos.Close();
            }
        }

        public void OnProviderDisabled(string provider)
        {
            //throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            //throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            //throw new NotImplementedException();
        }
    }
}
