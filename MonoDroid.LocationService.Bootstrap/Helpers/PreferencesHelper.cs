using Android.Content;
using Android.Preferences;

namespace DashCam.Classes.Helpers
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

      var value = _sharedPreferences.GetString(preferenceKey, "false");
      bool returnVal = bool.TryParse(value, out returnVal) && returnVal;
      return returnVal;
    }

    public static long GetPreferenceAsLong(Context context, string preferenceKey)
    {
      if (_sharedPreferences == null)
      {
        _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
      }

      var value = _sharedPreferences.GetString(preferenceKey, "0");
      long returnVal = long.TryParse(value, out returnVal) ? returnVal : 0;
      return returnVal;
    }

    public static int GetPreferenceAsInt(Context context, string preferenceKey)
    {
      if (_sharedPreferences == null)
      {
        _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
      }

      var value = _sharedPreferences.GetString(preferenceKey, "0");
      int returnVal = int.TryParse(value, out returnVal) ? returnVal : 0;
      return returnVal;
    }

    public static double GetPreferenceAsDouble(Context context, string preferenceKey)
    {
      if (_sharedPreferences == null)
      {
        _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
      }

      var value = _sharedPreferences.GetString(preferenceKey, "0");
      double returnVal = double.TryParse(value, out returnVal) ? returnVal : 0;
      return returnVal;
    }

    public static float GetPreferenceAsFloat(Context context, string preferenceKey)
    {
      if (_sharedPreferences == null)
      {
        _sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
      }

      var value = _sharedPreferences.GetString(preferenceKey, "0");
      float returnVal = float.TryParse(value, out returnVal) ? returnVal : 0;
      return returnVal;
    }
  }
}
