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

        private static void Notify(Exception ex)
        {
            BugSnag bs = new BugSnag();
            if (bs.ApiKey == "YOUR_API_KEY_HERE")
                throw new ArgumentException("ApiKey not set in app.config");

            bs.Notify(ex, new
                              {
                                  OtherReallyCoolData = new
                                                            {
                                                                color = "Yellow",
                                                                mood = "Mellow"
                                                            }
                              });
        }
    }
}
