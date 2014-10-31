using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    [DataContract]
    public class Device
    {
        /// <summary>
        /// The operating system version of the client that the error was
        /// generated on. (optional, default none)
        /// </summary>
        [DataMember(Name = "osVersion")]
        public string OsVersion { get; set; }

        /// <summary>
        /// The hostname of the server running your code
        /// (optional, default none) 
        /// </summary>
        [DataMember(Name = "hostname")]
        public string Hostname { get; set; }
    }
}
