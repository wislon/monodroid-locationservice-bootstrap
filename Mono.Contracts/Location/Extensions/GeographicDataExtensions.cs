using System;

namespace Mono.Contracts.Location.Extensions
{
    public static class GeographicDataExtensions
    {
        public static string MapToString(this GeographicData geographicData)
        {
            string locationInfo = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", 
                                                geographicData.Latitude, 
                                                geographicData.Longitude, 
                                                geographicData.Description,
                                                geographicData.CaptureTimeStamp.ToString("s"),
                                                DateTime.UtcNow.ToString("s"));
            return locationInfo;
        }
    }
}