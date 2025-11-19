using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProgPOEP1.Security
{
    // Usage on controllers: [AdminAuthorize("Coordinator")] or [AdminAuthorize("AcademicManager")]
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public AdminAuthorizeAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var isAdmin = http.Session.GetString("IsAdmin");
            var role = http.Session.GetString("Role");

            var authorized = isAdmin == "true" && role == _requiredRole;
            if (!authorized)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
