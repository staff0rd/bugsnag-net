using System;
using System.Configuration;
using System.Linq;
using System.Web;
using Bugsnag.Library.Data;

namespace Bugsnag.Library
{
    public class WebNotifier : Notifier
    {
        public App App { get; set; }

        public WebNotifier(string apiKey) : base(apiKey) {}

        public WebNotifier() : base() {}

        protected override void Configure()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["applicationVersion"]) || !string.IsNullOrEmpty(ConfigurationManager.AppSettings["BugSnagReleaseStage"]))
                App = new App { ReleaseStage = ConfigurationManager.AppSettings["BugSnagReleaseStage"], Version = ConfigurationManager.AppSettings["applicationVersion"] };

            base.Configure();
        }

        public void Notify()
        {
            this.Notify(null);
        }

        public void Notify(object extraData)
        {
            if(HttpContext.Current != null)
            {
                if (HttpContext.Current.AllErrors != null && HttpContext.Current.AllErrors.Any())
                {
                    base.Notify(
                        HttpContext.Current.AllErrors.ToList(),
                        HttpContext.Current.Request.Path,
                        null,
                        Severity.error, 
                        GetUser(),
                        App,
                        null,
                        extraData);
                }
                // TODO: Now what?
            }
            else
            {
                throw new NotImplementedException("Web application supported only at this time, use another method.");
            }
        }

        private User GetUser()
        {
            var user = new User();

            if(HttpContext.Current != null)
            {
                if(!string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                    user.Name = HttpContext.Current.User.Identity.Name;
                else if(HttpContext.Current.Session != null)
                    user.Id = HttpContext.Current.Session.SessionID ?? String.Empty;
            }

            return null;
        }
    }
}
