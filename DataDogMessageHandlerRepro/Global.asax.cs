using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Routing;

namespace DataDogMessageHandlerRepro
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.MessageHandlers.Add(new MyDelegatingHandler());
            GlobalConfiguration.Configuration.Services.Replace(typeof(IExceptionHandler), new MyExceptionHandler());
        }
    }
}
