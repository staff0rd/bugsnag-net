using System.Runtime.Serialization;

namespace Bugsnag.Library.Data
{
    //TODO: use a json serializer that will recognize EnumMember(Value=)

    [DataContract]
    public enum Severity
    {
        /// <summary>
        /// used when the app crashes
        /// </summary>
        //[EnumMember(Value = "error")]
        //Error,
        error,

        /// <summary>
        /// used when Bugsnag.notify is called
        /// </summary>
        //[EnumMember(Value = "warning")]
        //Warning,
        warning,

        /// <summary>
        /// can be used in manual Bugsnag.notify calls
        /// </summary>
        //[EnumMember(Value = "info")]
        //Info
        info
    }
}
