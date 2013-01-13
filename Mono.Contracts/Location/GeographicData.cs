using System;

namespace Mono.Contracts.Location
{
    public class GeographicData : GeographicBase
    {
        public string Description { get; set; }

        /// <summary>
        /// The timestamp of when the data was recorded. Not the same as the <see cref="CoordinateBase.TimeStamp"/> field, 
        /// which I am using to determine how long it was between when the data was captured and when it was stored.
        /// Think about how you store WHEN your object was captured, you may need to take time zones into account.
        /// </summary>
        public DateTime CaptureTimeStamp { get; set; }

    }
}