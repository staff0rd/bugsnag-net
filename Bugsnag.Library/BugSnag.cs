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
    public class BugSnag
    {
        private const string _httpUrl = "http://notify.bugsnag.com";

        private const string _httpsUrl = "https://notify.bugsnag.com";

        public string ApiKey { get; set; }

        public string ReleaseStage
        {
            get;
            set;
        }

        public List<string> NotifyReleaseStages
        {
            get;
            set;
        }

        public bool UseSSL { get; set; }

        public string ApplicationVersion { get; set; }

        public string OsVersion { get; set; }

        public BugSnag(string apiKey) : this()
        {
            this.ApiKey = apiKey;
        }

        public BugSnag()
        {
            UseSSL = false;

            //  Release stage defaults to 'production'
            ReleaseStage = "production";

            //  Notify release stages defaults to just notifying 
            //  for production
            NotifyReleaseStages = new List<string>();
            NotifyReleaseStages.Add("production");

            Configure();
        }

        private void Configure()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagApiKey"]))
                ApiKey = ConfigurationManager.AppSettings["BugSnagApiKey"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagUseSSL"]))
                UseSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["BugSnagUseSSL"]);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagReleaseStage"]))
                ReleaseStage = ConfigurationManager.AppSettings["BugSnagReleaseStage"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagNotifyReleaseStages"]))
            {
                NotifyReleaseStages.Clear();
                NotifyReleaseStages.AddRange(ConfigurationManager.AppSettings["BugSnagNotifyReleaseStages"].Split('|'));
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["applicationVersion"]))
                ApplicationVersion = ConfigurationManager.AppSettings["applicationVersion"];
        }

        public void Notify()
        {
            Notify(null);
        }

        public void Notify(object extraData)
        {
            if(HttpContext.Current != null)
            {
                if (HttpContext.Current.AllErrors != null && HttpContext.Current.AllErrors.Any())
                {
                    var events = new List<Event>
                                     {
                                         CreateEvent(
                                             HttpContext.Current.AllErrors.ToList(),
                                             HttpContext.Current.Request.Path,
                                             GetDefaultUserId(),
                                             extraData)
                                     };

                    SendNotification(events);
                }
            }
            else
            {
                throw new NotImplementedException("Web application supported only at this time, use another method.");
            }
        }

        public void Notify(System.Exception exception, object extraData) 
        {
            Notify(exception, string.Empty, string.Empty, extraData);
        }

        public void Notify(List<System.Exception> exceptions, object extraData)
        {
            Notify(exceptions, string.Empty, string.Empty, extraData);
        }

        public void Notify(System.Exception exception, string userId, string context, object extraData)
        {
            var exceptions = new List<System.Exception>();
            exceptions.Add(exception);
            Notify(exceptions, userId, context, extraData);
        }

        public void Notify(List<System.Exception> exList, string userId, string context, object extraData)
        {
            //  Add an event for this exception list:
            var events = new List<Event>();
            events.Add(CreateEvent(exList, context, userId, extraData));

            SendNotification(events);
        }

        private string GetDefaultUserId()
        {
            string userId = string.Empty;

            //  First, check to see if we have an HttpContext to work with
            if(HttpContext.Current != null)
            {
                //  If we have a current user, use that
                if(!string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                {
                    userId = HttpContext.Current.User.Identity.Name;
                }
                else if(HttpContext.Current.Session != null)
                {
                    //  Otherwise, use sessionID
                    userId = HttpContext.Current.Session.SessionID ?? String.Empty;
                }
            }
            else
                userId = Environment.UserName;
            
            return userId;
        }

        private Event CreateEvent(List<System.Exception> exList, string Context, string UserId, object extraData)
        {
            //  Create an event to return
            var retval = new Event()
            {
                AppVersion = this.ApplicationVersion,
                Context = Context,
                OSVersion = this.OsVersion,
                ReleaseStage = this.ReleaseStage,
                UserId = UserId,
                ExtraData = extraData
            };

            //  Our list of exceptions:
            var exceptions = new List<Bugsnag.Library.Data.Exception>();

            foreach(System.Exception ex in exList)
            {
                //  ... Create a list of stacktraces
                //  This may not be the best way to get this information:
                //  http://blogs.msdn.com/b/jmstall/archive/2005/03/20/399287.aspx
                var stacktraces = (from item in new System.Diagnostics.StackTrace(ex, true).GetFrames()
                                   select new Stacktrace()
                                   {
                                       File = item.GetFileName() ?? item.GetType().Name ?? "N/A",
                                       LineNumber = item.GetFileLineNumber(),
                                       Method = item.GetMethod().Name
                                   }).ToList();

                exceptions.Add(new Bugsnag.Library.Data.Exception()
                {
                    ErrorClass = ex.TargetSite.Name,
                    Message = ex.Message,
                    Stacktrace = stacktraces
                });
            }

            retval.Exceptions = exceptions;

            return retval;
        }

        private void SendNotification(List<Event> events)
        {
            var notification = new ErrorNotification()
            {
                ApiKey = this.ApiKey,
                Events = events
            };

            SendNotification(notification);
        }

        private void SendNotification(ErrorNotification notification)
        {
            string serializedJSON = notification.SerializeToString();

            //  Create a byte array:
            byte[] byteArray = Encoding.UTF8.GetBytes(serializedJSON);

            //  Post JSON to server:
            WebRequest request;
            if(UseSSL)
                request = WebRequest.Create(_httpsUrl);
            else
                request = WebRequest.Create(_httpUrl);

            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //  Get the response.  See https://bugsnag.com/docs/notifier-api for response codes
            var response = request.GetResponse() as HttpWebResponse;
            if ((int)response.StatusCode == 400)
                throw new HttpException(400, "The payload was too large or took too long (>10s) to read from the network.");
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpException((int)response.StatusCode, response.StatusDescription);
        }
    }
}
