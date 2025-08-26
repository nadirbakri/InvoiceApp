using System.Web.Mvc;
using System.Web.Routing;

namespace InvoiceWebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ControllerOnly",
                url: "{controller}",
                defaults: new { action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Invoices", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
