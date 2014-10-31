using System;
using Bugsnag.Library.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Exception = System.Exception;

namespace Bugsnag.Library.Tests
{
    [TestClass]
    public class NotifierTests
    {
        [TestMethod]
        public void MinimumJsonStructureTest()
        {
            var notifier = new Notifier();
            var minimumStructure = @"
            {
                ""apiKey"": """ + new BugSnag().ApiKey + @""",
                ""notifier"": {
                    ""name"": """ + notifier.Name + @""",
                    ""version"": """ + notifier.Version + @""",
                    ""url"": """ + notifier.Url + @"""
                },
                ""events"": [{
                    ""payloadVersion"": ""2"",
                    ""exceptions"": [{
                        ""errorClass"": """ + ErrorNotification.EmptyString + @""",
                        ""message"": """ + ErrorNotification.EmptyString + @""",
                        ""stacktrace"": [{
                            ""file"": """ + ErrorNotification.EmptyString + @""",
                            ""lineNumber"": 0,
                            ""method"": """ + ErrorNotification.EmptyString + @"""
                        }]
                    }]
                }]
            }";

            BugSnag.Send(minimumStructure, false);
        }

        [TestMethod]
        public void ApplicationExceptionTest()
        {
            try
            {
                throw new ApplicationException("Throwing an app exception.");
            }
            catch(Exception e)
            {
                GetNotifier().Notify(e);
            }
        }

        [TestMethod]
        public void BlankExceptionTest()
        {
            try
            {
                throw new Exception("Throwing an exception.");
            }
            catch (Exception e)
            {
                GetNotifier().Notify(e);
            }
        }

        [TestMethod]
        public void VersionTest()
        {
            var bs = GetNotifier();
            bs.ApplicationVersion = "1.0";
            bs.Notify(new Exception());
        }

        [TestMethod]
        public void SeverityInfoTest()
        {
            GetNotifier().Notify(new Exception("Info Severity"), severity: Severity.info);
        }
        
        [TestMethod]
        public void SeverityWarningTest()
        {
            GetNotifier().Notify(new Exception("Warning Severity"), severity: Severity.warning);
        }

        [TestMethod]
        public void EmailTest()
        {
            GetNotifier().Notify(new Exception("Email Test"), user: new User { Email = "test@email.com" });
        }

        private static BugSnag GetNotifier()
        {
            var bugsnag = new BugSnag();
            if (bugsnag.ApiKey == "YOUR_API_KEY_HERE")
                throw new ArgumentException("ApiKey not set in app.config");
            return bugsnag;
        }
    }
}
