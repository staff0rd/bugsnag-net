using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    [DataContract]
    public class User
    {
        /// <summary>
        /// A unique identifier for a user affected by this event. This could
        /// be any distinct identifier that makes sense for your
        /// application/platform.
        /// (optional, searchable)
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The user's name, or a string you use to identify them.
        /// (optional, searchable)
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The user's email address.
        /// (optional, searchable)
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }
    }
}
