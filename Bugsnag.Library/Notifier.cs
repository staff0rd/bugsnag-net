using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Bugsnag.Library.Data;
using ServiceStack.Text;

namespace Bugsnag.Library
{
    public class Notifier
    {
        protected const string HttpUrl = "http://notify.bugsnag.com";

        protected const string HttpsUrl = "https://notify.bugsnag.com";

        public string ApiKey { get; set; }

        public List<string> NotifyReleaseStages { get; set; }

        public bool UseSsl { get; set; }

        public Notifier(string apiKey, bool readConfig = true) : this()
        {
            ApiKey = apiKey;
        }

        public Notifier(bool readConfig = true)
        {
            UseSsl = false;

            NotifyReleaseStages = new List<string> { "production" };

            if (readConfig)
                Configure();
        }

        protected virtual void Configure()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagApiKey"]))
                ApiKey = ConfigurationManager.AppSettings["BugSnagApiKey"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagUseSSL"]))
                UseSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["BugSnagUseSSL"]);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagNotifyReleaseStages"]))
            {
                NotifyReleaseStages.Clear();
                NotifyReleaseStages.AddRange(ConfigurationManager.AppSettings["BugSnagNotifyReleaseStages"].Split('|'));
            }
        }

        public void Notify(System.Exception exception, object extraData = null)
        {
            Notify(CreateEvent(exception, extraData));
        }

        public void Notify(System.Exception exception, string context, string groupingHash, Severity severity, User user, App app, Device device, object extraData = null)
        {
            Notify(new[] { exception }, context, groupingHash, severity, user, app, device, extraData);
        }

        public void Notify(IList<System.Exception> exceptions, string context, string groupingHash, Severity severity, User user, App app, Device device, object extraData = null)
        {
            var theEvent = CreateEvent(exceptions, context, groupingHash, severity, user, app, device, extraData);
            Notify(theEvent);
        }

        public void Notify(Event ev)
        {
            Notify(new[] { ev });
        }

        public void Notify(IList<Event> events)
        {
            var notification = new ErrorNotification
            {
                ApiKey = ApiKey,
                Events = events
            };

            SendNotification(notification);
        }

        public static Event CreateEvent(System.Exception exception, object extraData = null)
        {
            return CreateEvent(
                new[] { exception },
                null,
                null,
                Severity.error,
                null,
                new App { ReleaseStage = "production" },
                null,
                extraData
                );
        }

        public static Event CreateEvent(IList<System.Exception> exceptions, string context, string groupingHash, Severity severity, User user, App app, Device device, object extraData)
        {
            return new Event
            {
                Exceptions = exceptions.Select(p => p.ToBugsnagException()).ToList(),
                Context = context,
                GroupingHash = groupingHash,
                Severity = severity,
                App = app,
                User = user,
                Device = device,
                MetaData = extraData
            };
        }

        private void SendNotification(ErrorNotification notification)
        {
            Send(notification.ToJson(), UseSsl);
        }

        public static void Send(string serializedJson, bool useSsl)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(serializedJson);

            var request = WebRequest.Create(useSsl ? HttpsUrl : HttpUrl);

            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            ValidateResponse(request);
        }

        private static void ValidateResponse(WebRequest request)
        {
            //  Get the response.  See https://bugsnag.com/docs/notifier-api for response codes
            var response = request.GetResponse() as HttpWebResponse;
            if ((int)response.StatusCode == 400)
                throw new HttpException(400, "The payload was too large or took too long (>10s) to read from the network.");
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpException((int)response.StatusCode, response.StatusDescription);
        }
    }
}
