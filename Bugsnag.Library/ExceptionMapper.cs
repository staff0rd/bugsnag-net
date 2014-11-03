using System.Collections.Generic;
using System.Linq;
using Bugsnag.Library.Data;
using Exception = Bugsnag.Library.Data.Exception;

namespace Bugsnag.Library
{
    public static class ExtensionMapper {

        public static Exception ToBugsnagException(this System.Exception systemException)
        {
                //  This may not be the best way to get this information:
                //  http://blogs.msdn.com/b/jmstall/archive/2005/03/20/399287.aspx
                var stacktraces = default(List<Stacktrace>);
                var frames = new System.Diagnostics.StackTrace(systemException, true).GetFrames();
                if (frames != null)
                {
                    stacktraces = frames.Select(item => new Stacktrace()
                                                            {
                                                                File = item.GetFileName() ?? item.GetType().Name,
                                                                LineNumber = item.GetFileLineNumber(),
                                                                Method = item.GetMethod().Name
                                                            }).ToList();
                }

                var exception = new Exception
                {
                    Message = systemException.Message,
                    Stacktrace = stacktraces ?? new List<Stacktrace> { new Stacktrace() }
                };

                if (systemException.TargetSite != null)
                    exception.ErrorClass = systemException.TargetSite.Name;
                else if (!string.IsNullOrWhiteSpace(systemException.Message))
                    exception.ErrorClass = systemException.Message;
                else
                    exception.ErrorClass = ErrorNotification.EmptyString;

            return exception;
        }
    }
}
