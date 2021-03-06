﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    [DataContract]
    public class ErrorNotification
    {
        public const string EmptyString = "Undefined";

        public ErrorNotification()
        {
            Notifer = new Notifier();
        }

        /// <summary>
        /// The API Key associated with the project. Informs Bugsnag which project 
        /// has generated this error.
        /// </summary>
        [DataMember(Name = "apiKey")]
        public string ApiKey { get; set; }

        /// <summary>
        /// This object describes the notifier itself. These properties are used 
        /// within Bugsnag to track error rates from a notifier.
        /// </summary>
        [DataMember(Name = "notifier")]
        public Notifier Notifer { get; set; }

        /// <summary>
        /// An array of error events that Bugsnag should be notified of. A notifier
        /// can choose to group notices into an array to minimize network traffic, or
        /// can notify Bugsnag each time an event occurs. 
        /// </summary>
        [DataMember(Name = "events")]
        public IList<Event> Events { get; set; }
    }
}
