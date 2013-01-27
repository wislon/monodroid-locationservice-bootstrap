﻿using Android.App;
using Android.Content;
using Android.Locations;
using Android.Util;
using Android.Widget;
using Android.OS;
using MonoDroid.LocationService.Bootstrap.BroadcastReceivers;
using MonoDroid.LocationService.Bootstrap.Services;

namespace MonoDroid.LocationService.Bootstrap
{
    [Activity(Label = "Location Monitoring", MainLauncher = true, Icon = "@drawable/Icon")]
    public class LMSActivity : Activity
    {
        private bool _serviceStarted;
        private Button _btnServiceControl;
        private Button _btnServiceActive;
        private Button _btnExportData;
        private TextView _tvGpsStatus;

        private const int ReturnFromGpsSettingActivity = 1;

        private MainActivityBroadcastReceiver _mabReceiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            SetupControls();

            _mabReceiver = new MainActivityBroadcastReceiver();
            _mabReceiver.Receive += HandleBroadcastMessages;

            UpdateUI();
        }

        private void SetupControls()
        {
            _btnServiceControl = FindViewById<Button>(Resource.Id.btnServiceControl);

            _btnServiceControl.Click += (s, e) => ToggleServiceActive();

            _btnServiceActive = FindViewById<Button>(Resource.Id.btnServiceActive);
            _btnServiceActive.Click += (sender, args) => CheckIfLocationMonitoringServiceIsActive();

            _btnExportData = FindViewById<Button>(Resource.Id.btnExportData);
            _btnExportData.Click += (sender, args) => ExportData();

            _tvGpsStatus = FindViewById<TextView>(Resource.Id.tvGpsStatus);
        }

        private void ToggleServiceActive()
        {
            var intent = new Intent(this, typeof (LocationMonitoringService));
            if (!_serviceStarted)
            {
                if (CheckAndEnableGps())
                {
                    StartService(intent);
                    _serviceStarted = true;
                }
            }
            else
            {
                StopService(intent);
                _serviceStarted = false;
            }

            UpdateUI();
        }

        private bool CheckAndEnableGps()
        {
            if (IsGpsEnabled()) return true;

            new AlertDialog.Builder(this)
                .SetTitle("GPS service not enabled")
                .SetMessage("GPS location service is disabled in your system preferences, please enable before continuing")
                .SetPositiveButton("Enable GPS", (s, e) =>
                                                     {
                                                         // load up the system's location settings so the user can turn on the GPS service
                                                         var showGpsSettingsIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                                                         StartActivityForResult(showGpsSettingsIntent, ReturnFromGpsSettingActivity);
                                                     })
                .SetNegativeButton("Cancel", (sender, args) => {  })
                .SetCancelable(true)
                .Show();

            return false;
        }

        private bool IsGpsEnabled()
        {
            var locationManager = (LocationManager) GetSystemService(Context.LocationService);
            return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case ReturnFromGpsSettingActivity:
                    {
                        UpdateUI();
                        break;
                    }
            }
        }

        private void ExportData()
        {
            Log.Info("LMSA.ExportData", string.Format("Exporting data..."));
            var exportIntent = new Intent(AppConstants.SERVICE_COMMAND);
            exportIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int) AppConstants.ServiceCommandType.ExportData);
            SendBroadcast(exportIntent);
            Log.Info("LMSA.ExportData", string.Format("Export message sent"));
        }


        private void HandleBroadcastMessages(Context context, Intent intent)
        {
            Log.Info("LMSA.HandleBroadcastMessages", string.Format("Message received: {0}", intent.Action));

            if (intent.Action == AppConstants.APPLICATION_COMMAND) // pong!
            {
                var commandType = (AppConstants.ApplicationCommandType) intent.GetIntExtra(AppConstants.COMMAND_TYPE_ID, -1);
                switch (commandType)
                {
                    case AppConstants.ApplicationCommandType.ReceivePong:
                        {
                            Log.Info("LMSA.HandleResponseMessages", string.Format("Service is active"));
                            _serviceStarted = true;
                            UpdateUI();
                            break;
                        }
                    case AppConstants.ApplicationCommandType.DataExported:
                        {
                            Log.Info("LMSA.HandleResponseMessages", string.Format("Data exported successfully"));
                            Toast.MakeText(this, "Exported successfully", ToastLength.Short).Show();
                            break;
                        }
                }

            }
        }

        private void UpdateUI()
        {
            bool gpsIsEnabled = IsGpsEnabled();
            _tvGpsStatus.Text = string.Format("GPS Status: {0}", gpsIsEnabled ? "Enabled" : "Disabled");
            _btnServiceControl.Text = _serviceStarted || (!gpsIsEnabled && _serviceStarted) ? "Stop Service" : "Start Service";
            if (!gpsIsEnabled && _serviceStarted) { ToggleServiceActive();}
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Info("LMSA.OnResume", "Registering receivers...");
            RegisterReceiver(_mabReceiver, new IntentFilter(AppConstants.APPLICATION_COMMAND));
            CheckIfLocationMonitoringServiceIsActive();
            UpdateUI();
        }

        /// <summary>
        /// Ping the service to see if it's running, if it is, we'll get a response, if not, 
        /// then nothing. It will not (re)start the service if it's not running.
        /// </summary>
        private void CheckIfLocationMonitoringServiceIsActive()
        {
            Log.Info("LMSA", "Check if Location Monitoring Service Is Active");
            var pingIntent = new Intent(AppConstants.SERVICE_COMMAND);
            pingIntent.PutExtra(AppConstants.COMMAND_TYPE_ID, (int) AppConstants.ServiceCommandType.SendPing);
            SendBroadcast(pingIntent);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Log.Info("LMSA.OnPause", "Deregistering receivers...");
            this.UnregisterReceiver(_mabReceiver);
        }

    }
}

