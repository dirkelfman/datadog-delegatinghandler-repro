using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace DataDogMessageHandlerRepro
{
    public class MyDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage retVal = null;

            try
            {
                retVal = await base.SendAsync(request, cancellationToken);
            }
            //should hit this branch
            catch (MyException)
            {
                retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "correct Exception was thrown" });
            }
            //should not hit this branch.
            catch (AggregateException)
            {
                retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "with datadog trace running an AggregateException is thrown" });
            }
            return retVal;
        }
    }
    public class MyExceptionHandler : ExceptionHandler
    {
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return false;
        }
    }
    public class MyException : Exception
    {


    }
}