using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TweetrSearchWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null) return;

            var context = new HttpContextWrapper(System.Web.HttpContext.Current);
            var routeData = RouteTable.Routes.GetRouteData(context);

            var requestContext = new RequestContext(context, routeData);
            /* when the request is ajax the system can automatically handle a mistake with a JSON response. then overwrites the default response */
            if (requestContext.HttpContext.Request.IsAjaxRequest())
            {
                httpContext.Response.Clear();
                var controllerName = requestContext.RouteData.GetRequiredString("controller");
                var factory = ControllerBuilder.Current.GetControllerFactory();
                var controller = factory.CreateController(requestContext, controllerName);
                var controllerContext = new ControllerContext(requestContext, (ControllerBase)controller);

                var jsonResult = new JsonResult
                {
                    Data = new {success = false, serverError = "500"},
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                jsonResult.ExecuteResult(controllerContext);
                httpContext.Response.End();
            }
            else
            {
                httpContext.Response.Redirect("~/Error");
            }
        }
    }
}
