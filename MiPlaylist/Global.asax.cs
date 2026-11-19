using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MiPlaylist
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

        // Cuando el servidor crea una sesión nueva (por ejemplo tras reiniciar),
        // se reconstruye desde la cookie cifrada de "recordarme"
        protected void Session_Start()
        {
            MiPlaylist.Helpers.SesionPersistente.Restaurar(HttpContext.Current);
        }
    }
}