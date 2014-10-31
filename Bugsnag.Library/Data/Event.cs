using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    /// <summary>
    /// Error event that Bugsnag should be notified of. A notifier
    /// can choose to group notices into an array to minimize network traffic, or
    /// can notify Bugsnag each time an event occurs
    /// </summary>
    [DataContract]
    public class Event
    {
        public Event()
        {
            Exceptions = new List<Exception>();
        }

        // The version number of the payload. If not set to 2+, Severity will
        // not be supported.
        // (required, must be set to "2")
        [DataMember(Name = "payloadVersion")]
        public string PayloadVersion
        {
            get
            {
                return "2";
            }
        }

        /// <summary>
        /// An array of exceptions that occurred during this event. Most of the
        /// time there will only be one exception, but some languages support 
        /// "nested" or "caused by" exceptions. In this case, exceptions should 
        /// be unwrapped and added to the array one at a time. The first exception
        /// raised should be first in this array.
        /// </summary>
        [DataMember(Name = "exceptions")]
        public List<Exception> Exceptions { get; set; }

        // Information about the app that crashed.
        // These fields are optional but highly recommended
        [DataMember(Name = "app")]
        public App App { get; set; }
    }
}
