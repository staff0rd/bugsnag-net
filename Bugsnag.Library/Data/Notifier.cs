using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bugsnag.Library.Data
{
    /// <summary>
    /// Notifier meta data
    /// </summary>
    [DataContract]
    public class Notifier
    {
        public Notifier()
        {
            Name = "Bugsnag .NET";
            Version = "0.2";
            Url = "https://github.com/staff0rd/bugsnag-net";
        }

        /// <summary>
        /// The notifier name
        /// </summary>
        [DataMember(Name="name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The notifier's current version
        /// </summary>
        [DataMember(Name = "version")]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// The URL associated with the notifier
        /// </summary>
        [DataMember(Name = "url")]
        public string Url
        {
            get;
            set;
        }
    }
}
