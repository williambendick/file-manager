using System.Web.Mvc;
using System.Web.Routing;

namespace file_manager
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("", "api/MoveItems", new { controller = "API", action = "MoveItems" });

            routes.MapRoute("", "api/CopyItems", new { controller = "API", action = "CopyItems" });

            routes.MapRoute("", "api/DeleteItems", new { controller = "API", action = "DeleteItems" });

            routes.MapRoute("", "api/{*apiPath}", new { controller = "API", action = "GetDirectoryItems" });
           
            routes.MapRoute("", "download/{*apiPath}", new { controller = "Download", action = "DownloadFile" });

            routes.MapRoute("", "upload/{*apiPath}", new { controller = "Upload", action = "UploadFiles" });

            routes.MapRoute("", "{*url}", new { controller = "Default", action = "Index" });
        }
    }
}
