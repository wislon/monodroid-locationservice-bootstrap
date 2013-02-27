using Android.App;
using Android.Preferences;

namespace MonoDroid.LocationService.Bootstrap
{
    [Activity(Label="User Preferences" )]
    public class UserPreferencesActivity : PreferenceActivity
    {
        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.AddPreferencesFromResource(Resource.Layout.UserPreferences);
        }

         
    }
}