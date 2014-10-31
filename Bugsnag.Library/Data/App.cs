using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    /// <summary>
    /// Information about the app that crashed.
    /// These fields are optional but highly recommended
    /// </summary>
    [DataContract]
    public class App
    {
        /// <summary>
        /// The version number of the application which generated the error.
        /// If appVersion is set and an error is resolved in the dashboard
        /// the error will not unresolve until a crash is seen in a newer
        /// version of the app.
        /// (optional, default none, filtered)
        /// </summary>
        [DataMember(Name="version")]
        public string Version { get; set; }

        /// <summary>
        /// The release stage that this error occurred in, for example
        /// "development", "staging" or "production".
        /// (optional, default "production", filtered)
        /// </summary>
        [DataMember(Name="releaseStage")]
        public string ReleaseStage { get; set; }
    }
}
