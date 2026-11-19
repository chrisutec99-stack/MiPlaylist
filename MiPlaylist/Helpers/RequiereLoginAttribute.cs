using System.Web.Mvc;
using System.Web.Routing;

namespace MiPlaylist.Helpers
{
    public class RequiereLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["UsuarioId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
        }
    }
}