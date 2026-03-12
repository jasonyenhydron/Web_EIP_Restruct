using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web_EIP_Restruct.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireLogonAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var username = context.HttpContext.Session.GetString("username");
        if (!string.IsNullOrWhiteSpace(username))
        {
            base.OnActionExecuting(context);
            return;
        }

        context.Result = new RedirectToActionResult("Logon", "Account", null);
    }
}
