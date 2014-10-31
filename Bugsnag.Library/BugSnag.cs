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

using Exception = Bugsnag.Library.Data.Exception;

namespace Bugsnag.Library
{
    public class BugSnag
    {
        private static string HttpUrl = "http://notify.bugsnag.com";

        private const string HttpsUrl = "https://notify.bugsnag.com";

        public string ApiKey { get; set; }

        public string ReleaseStage { get; set; }

        public List<string> NotifyReleaseStages { get; set; }

        public bool UseSSL { get; set; }

        public string ApplicationVersion { get; set; }

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

        public void WebNotify()
        {
            WebNotify(null);
        }

        public void WebNotify(object extraData)
        {
            if(HttpContext.Current != null)
            {
                if (HttpContext.Current.AllErrors != null && HttpContext.Current.AllErrors.Any())
                {
                    Notify(
                        HttpContext.Current.AllErrors.ToList(),
                        HttpContext.Current.Request.Path,
                        extraData: extraData);
                }
                // TODO: Now what?
            }
            else
            {
                throw new NotImplementedException("Web application supported only at this time, use another method.");
            }
        }

        public void Notify(System.Exception exception, string context = null, string groupingHash = null, Severity? severity = null, User user = null, Device device = null, object extraData = null)
        {
            Notify(new[] { exception }, context, groupingHash, severity, user, device, extraData);
        }

        public void Notify(IList<System.Exception> exList, string context = null, string groupingHash = null, Severity? severity = null, User user = null, Device device = null, object extraData = null)
        {
            var events = new List<Event>();
            events.Add(CreateEvent(exList, context, groupingHash, severity, user, device, extraData));

            var notification = new ErrorNotification
            {
                ApiKey = ApiKey,
                Events = events
            };

            SendNotification(notification);
        }

        private string GetDefaultUserId()
        {
            string userId = string.Empty;

            if(HttpContext.Current != null)
            {
                if(!string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                {
                    userId = HttpContext.Current.User.Identity.Name;
                }
                else if(HttpContext.Current.Session != null)
                {
                    userId = HttpContext.Current.Session.SessionID ?? String.Empty;
                }
            }
            else
                userId = Environment.UserName;
            
            return userId;
        }

        private Event CreateEvent(IList<System.Exception> exceptions, string context = null, string groupingHash = null, Severity? severity = null, User user = null, Device device = null, object extraData = null)
        {
            var retval = new Event
            {
                Exceptions = MapExceptions(exceptions),
                Context = context,
                GroupingHash = groupingHash,
                Severity = severity ?? Severity.error,
                App = new App { ReleaseStage = ReleaseStage, Version = ApplicationVersion },
                User = user ?? new User(),
                Device = device
            };

            if (retval.User.Id == null)
                retval.User.Id = GetDefaultUserId();

            if (retval.Exceptions.Any())
            {
                if (retval.Context == null)
                    retval.Context = retval.Exceptions.First().Stacktrace.First().File;
                if (retval.GroupingHash == null)
                    retval.GroupingHash = retval.Exceptions.First().Stacktrace.First().File;
            }

            return retval;
        }

        private static List<Exception> MapExceptions(IList<System.Exception> exceptions)
        {
            var bugsnagExceptions = new List<Bugsnag.Library.Data.Exception>();

            foreach (System.Exception ex in exceptions)
            {
                //  ... Create a list of stacktraces
                //  This may not be the best way to get this information:
                //  http://blogs.msdn.com/b/jmstall/archive/2005/03/20/399287.aspx
                var stacktraces = default(List<Stacktrace>);
                var frames = new System.Diagnostics.StackTrace(ex, true).GetFrames();
                if (frames != null)
                {
                    stacktraces = frames.Select(item => new Stacktrace()
                                                            {
                                                                File = item.GetFileName() ?? item.GetType().Name,
                                                                LineNumber = item.GetFileLineNumber(),
                                                                Method = item.GetMethod().Name
                                                            }).ToList();
                }

                bugsnagExceptions.Add(new Bugsnag.Library.Data.Exception()
                                          {
                                              ErrorClass = ex.TargetSite == null ? "Undefined" : ex.TargetSite.Name,
                                              Message = ex.Message,
                                              Stacktrace = stacktraces ?? new List<Stacktrace> { new Stacktrace { File = "test", LineNumber = 0, Method = "test" } }
                                          });
            }
            return bugsnagExceptions;
        }

        private void SendNotification(ErrorNotification notification)
        {
            Send(notification.SerializeToString(), UseSSL);
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
