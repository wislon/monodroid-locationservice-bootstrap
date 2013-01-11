using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using Android.OS;
using MonoDroid.LocationService.Bootstrap.Services;

namespace MonoDroid.LocationService.Bootstrap
{
    [Activity(Label = "Location Monitoring", MainLauncher = true, Icon = "@drawable/icon")]
    public class LMSActivity : Activity
    {
        private bool _serviceStarted;
        private Button _btnServiceControl;
        private Button _btnServiceActive;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            SetupControls();
        }

        private void SetupControls()
        {
            _btnServiceControl = FindViewById<Button>(Resource.Id.btnServiceControl);

            _btnServiceControl.Click += (s, e) =>
                                {
                                    var intent = new Intent(this, typeof(LocationMonitoringService));
                                    //var intent = new Intent(this, typeof(TestService));
                                    if (!_serviceStarted)
                                    {
                                        StartService(intent);
                                    }
                                    else
                                    {
                                        StopService(intent);
                                    }

                                    _serviceStarted = !_serviceStarted;
                                    UpdateUI();
                                };

            _btnServiceActive = FindViewById<Button>(Resource.Id.btnServiceActive);
            _btnServiceActive.Click += (sender, args) => CheckIfLocationMonitoringServiceIsActive();
        }


        //private void HandleResponseMessages(Context context, Intent intent)
        //{
        //    Log.Info("LMSA.HandleResponseMessages", string.Format("Message received: {0}", intent.Action));

        //    if (intent.Action == LocationMonitoringService.SERVICE_IS_ACTIVE) // pong!
        //    {
        //        Log.Info("LMSA.HandleResponseMessages", string.Format("Service is active"));
        //        _serviceStarted = true;
        //        UpdateUI();
        //    }
        //}

        private void UpdateUI()
        {
            _btnServiceControl.Text = !_serviceStarted ? "Start Service" : "Stop Service";
        }

        protected override void OnResume()
        {
            base.OnResume();
            //Log.Info("LMSA.OnResume", "Registering receivers...");
            // RegisterReceiver(_lmsrReceiver, new IntentFilter(LocationMonitoringService.SERVICE_IS_ACTIVE));
            // CheckIfLocationMonitoringServiceIsActive();
        }

        /// <summary>
        /// Ping the service to see if it's running, if it is, we'll get a response, if not, 
        /// then nothing. It will not (re)start the service if it's not running.
        /// </summary>
        private void CheckIfLocationMonitoringServiceIsActive()
        {
            Log.Info("LMSA", "Check If Location Monitoring Service Is Active");
            var intent = new Intent(ApplicationConstants.SERVICE_COMMAND);
            intent.PutExtra(ApplicationConstants.COMMAND_TYPE_ID, (int) ApplicationConstants.ServiceCommandType.SendPing);
            SendBroadcast(intent);
        }

        protected override void OnPause()
        {
            base.OnPause();
            //Log.Info("LMSA.OnPause", "Deregistering receivers...");
            //this.UnregisterReceiver(_lmsrReceiver);
        }

    }
}

