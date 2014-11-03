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
            var notifier = new Bugsnag.Library.Data.Notifier();
            var minimumStructure = @"
            {
                ""apiKey"": """ + new Notifier().ApiKey + @""",
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

            Notifier.Send(minimumStructure, false);
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
                Notify(e);
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
                Notify(e);
            }
        }

        [TestMethod]
        public void VersionTest()
        {
            var ev = GetEvent();
            ev.App = new App { Version = "2.0" };
            Notify(ev);
        }

        [TestMethod]
        public void SeverityInfoTest()
        {
            var ev = GetEvent("Info Severity");
            ev.Severity = Severity.info;
            Notify(ev);
        }
        
        [TestMethod]
        public void SeverityWarningTest()
        {
            var ev = GetEvent("Warning Severity");
            ev.Severity = Severity.warning;
            Notify(ev);
        }

        [TestMethod]
        public void EmailTest()
        {
            var ev = GetEvent("Email test");
            ev.User = new User { Email = "test@email.com" };
            Notify(ev);
        }

        [TestMethod]
        public void OsVersionTest()
        {
            var ev = GetEvent("Os Version Test");
            ev.Device = new Device { OsVersion = "1.1.1" };
            Notify(ev);
        }

        [TestMethod]
        public void HostnameTest()
        {
            var ev = GetEvent("Hostname Test");
            ev.Device = new Device { Hostname = "my.host.name" };
            Notify(ev);
        }

        [TestMethod]
        public void MetaDataTest()
        {
            var ev = GetEvent("MetaData Test");
            ev.MetaData = new
                              {
                                  OtherReallyCoolData = new
                                                            {
                                                                color = "Red",
                                                                mood = "Mellow"
                                                            }
                              };
        }

        private Event GetEvent(string message = null)
        {
            return Notifier.CreateEvent(new Exception(message));
        }

        private static void Notify(System.Exception exception)
        {
            Notify(Notifier.CreateEvent(exception));
        }

        private static void Notify(Event ev)
        {
            var notifier = new Notifier();
            if (notifier.ApiKey == "YOUR_API_KEY_HERE")
                throw new ArgumentException("ApiKey not set in app.config");
            notifier.Notify(ev);
        }
    }
}
