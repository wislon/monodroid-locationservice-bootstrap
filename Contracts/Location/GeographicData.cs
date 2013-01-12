using System;

namespace Contracts.Location
{
    public class GeographicData : GeographicBase
    {
        public string Description { get; set; }

        /// <summary>
        /// The timestamp of when the data was recorded. Not the same  as the 
        /// <see cref="CoordinateBase.TimeStamp"/>  field. 
        /// You'll need to be careful  about how you store when your object was 
        /// populated, taking timezones into account if you need to.
        /// </summary>
        public DateTime CaptureTimeStamp { get; set; }
    }
}