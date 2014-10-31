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
        public void ApplicationExceptionTest()
        {
            try
            {
                throw new ApplicationException("Throwing an app extension.  You heartless bastard.");
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
                throw new Exception();
            }
            catch (Exception e)
            {
                Notify(e);
            }
        }

        private static void Notify(Exception ex)
        {
            BugSnag bs = new BugSnag();

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
