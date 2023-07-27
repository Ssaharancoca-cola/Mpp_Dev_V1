using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MPP.Filter
{
    public class SessionTimeoutDimensionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            if (session == null || !session.TryGetValue("SelectedDimensionData", out _) || !session.TryGetValue("SelectedDimensionValue", out _))
            {
                context.Result = new RedirectToActionResult("SessionExpire", "Home", null);
            }
        }        
    }
    public class SessionTimeoutEntityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context) 
        {
            var session = context.HttpContext.Session;
            if (session == null || !session.TryGetValue("EntityTypeID", out _) || !session.TryGetValue("EntityName", out _))
            {
                context.Result = new RedirectToActionResult("SessionExpire", "Home", null);
            }
        }
    }
    public class SessionTimeoutPagingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var session = context.HttpContext.Session;
            if (session == null || !session.TryGetValue("EntityTypeID", out _) || !session.TryGetValue("currentPageNo", out _) || !session.TryGetValue("fieldCollection", out _))
            {
                context.Result = new RedirectToActionResult("SessionExpire", "Home", null);
            }
        }
    }
}
