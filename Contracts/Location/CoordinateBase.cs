using System;

namespace Contracts.Location
{
    /// <summary>
    /// Co-ordinate base class. Bare minimum of info to be recorded, namely an ID
    /// and a timestamp (which will be recorded in UTC time in our implementation).
    /// </summary>
    public abstract class CoordinateBase : ICoordinateBase
    {
        public string Id { get; set; }

        /// <summary>
        /// Internal use only - used for tracking when an object was placed in the persistence store. 
        /// Provide your own DateTime-based property in a derived class if you want to track operation-specific 
        /// time-based operations on your objects. 
        /// </summary>
        public DateTime TimeStamp { get; set; }

    }
}