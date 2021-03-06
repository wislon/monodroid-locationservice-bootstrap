using System;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Widget;
using Mono.Contracts.Location;
using MonoDroid.LocationService.Bootstrap.BroadcastReceivers;
using MonoDroid.LocationService.Bootstrap.Persistence;

namespace MonoDroid.LocationService.Bootstrap.Services
{
    [Service(Label = "Location Monitoring")]
    public class LocationMonitoringService : Service, ILocationListener
    {
        private IRepository<GeographicData> _repository;

        private NotificationManager _notificationManager;
        private string _bestProvider;
        private LocationManager _locationManager;

        private ServiceBroadcastReceiver _sbr;
        private bool _exporting;


        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }


        public override void OnCreate()
        {
            Log.Info("LMService.OnCreate", string.Format("In OnCreate"));
            base.OnCreate();

            _repository = new Repository<GeographicData>(this);


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
                                                       pongIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int)AppConstants.ApplicationCommandType.ReceivePong);
                                                       Log.Info("TestService", "Sending pong!");
                                                       SendBroadcast(pongIntent);
                                                       break;
                                                   }
                                               case AppConstants.ServiceCommandType.StopService:
                                                   {
                                                       Log.Info("TestService", "Service stopping...");
                                                       ExportData();
                                                       StopSelf();
                                                       break;
                                                   }
                                               case AppConstants.ServiceCommandType.ExportData:
                                                   {
                                                       ExportData();
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
                                   PowerRequirement = Power.Medium,
                                   AltitudeRequired = false,
                                   BearingRequired = false,
                                   SpeedRequired = false,
                                   CostAllowed = false,
                               };
            _locationManager = (LocationManager) GetSystemService(Context.LocationService);
            
            // !!! Remember to enable the location permissions in the project attributes or _bestProvider comes back null :)
            // you will also need to ensure that the container class either inherits from Service or Activity, or it goes all crashy;
            // but you'll see a build warning about it implementing IJavaObject but not inheriting from Java.Object
            _bestProvider = _locationManager.GetBestProvider(criteria, false);
            // Location _location = _locationManager.GetLastKnownLocation(_bestProvider);
            // at least 15 seconds between updates, at least 100 metres between updates, 'this' because it implements ILocationListener.
            _locationManager.RequestLocationUpdates(_bestProvider, 15000, 100, this);

        }

        private void ExportData()
        {
            try
            {
                if (!_exporting)
                {
                    _exporting = true;
                    string fileName = _repository.ExportData();
                    var exportedIntent = new Intent(AppConstants.APPLICATION_COMMAND);
                    exportedIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int)AppConstants.ApplicationCommandType.DataExported);
                    exportedIntent.PutExtra(AppConstants.EXPORTED_FILE_NAME, fileName);
                    SendBroadcast(exportedIntent);
                }
            }
            finally
            {
                _exporting = false;
            }
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info("LMService.OnStartCommand", "In OnStartCommand");
            base.OnStartCommand(intent, flags, startId);

            Log.Info("LMService.OnStartCommand", "Received start ID {0} : {1}", startId, intent);
            Log.Info("LMService.OnStartCommand", "Registering broadcast receivers...");
            RegisterReceiver(_sbr, new IntentFilter(AppConstants.SERVICE_COMMAND)); 

            // so we don't close if/when the app does. We have to stop it specifically.
            return StartCommandResult.Sticky; 
        }


        public override void OnDestroy()
        {
            Log.Info("LMService.OnDestroy", "In OnDestroy");
            ExportData();
            _notificationManager.Cancel(Resource.String.LocationMonitoringServiceStarted);
            Toast.MakeText(this, Resource.String.LocationMonitoringServiceStopped, ToastLength.Short).Show();
            _locationManager.RemoveUpdates(this);
            UnregisterReceiver(_sbr);
            StopSelf();
            base.OnDestroy();
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

            // This sets up an intent that will be attached to the notification, so when you click the 
            // notification in the notification bar, it'll bring us back to 'LMSActivity' (our 'launcher')
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
            TriggerStandardNotification(text); // this can get annoying after a bit, but it's useful to see if the service is still running and capturing.
            WriteLocationFileEntry(location);
        }

        private void WriteLocationFileEntry(Location location)
        {
            var locationInfo = new GeographicData()
                                   {
                                       // leaving Id and Timestanp out, will let the persistence logic take care of that
                                       Latitude = location.Latitude,
                                       Longitude = location.Longitude,
                                       CaptureTimeStamp = DateTime.UtcNow,
                                       Description = string.Format("Recorded at {0}", DateTime.UtcNow.ToString("s")),
                                   };

            _repository.Insert(locationInfo);
            if (!_exporting) // so we still queue up the data, just don't write it if the current data is being exported.
            {
                _repository.SaveChanges();
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
