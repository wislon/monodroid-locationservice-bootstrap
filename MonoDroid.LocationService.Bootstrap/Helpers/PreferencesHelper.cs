using Android.Content;
using Android.Preferences;

namespace MonoDroid.LocationService.Bootstrap.Helpers
{
    public static class PreferencesHelper
    {
        private static ISharedPreferences _sharedPreferences = null;

        public static string GetPreferenceAsString(Context context, string preferenceKey)
        {
            if (_sharedPreferences == null)
            {
                _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            var value = _sharedPreferences.GetString(preferenceKey, string.Empty);
            return value;
        }

        public static bool GetPreferenceAsBool(Context context, string preferenceKey)
        {
            if (_sharedPreferences == null)
            {
                _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            var value = _sharedPreferences.GetBoolean(preferenceKey, false);
            return value;
        }

        public static long GetPreferenceAsLong(Context context, string preferenceKey)
        {
            if (_sharedPreferences == null)
            {
                _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            var value = _sharedPreferences.GetLong(preferenceKey, 0);
            return value;
        }

        public static int GetPreferenceAsInt(Context context, string preferenceKey)
        {
            if (_sharedPreferences == null)
            {
                _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            var value = _sharedPreferences.GetInt(preferenceKey, 0);
            return value;
        }

        public static float GetPreferenceAsFloat(Context context, string preferenceKey)
        {
            if (_sharedPreferences == null)
            {
                _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            }

            var value = _sharedPreferences.GetFloat(preferenceKey, 0);
            return value;
        }
    }
}