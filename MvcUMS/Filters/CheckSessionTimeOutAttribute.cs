using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MvcUMS.Filters
{
    //[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CheckSessionTimeOutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext context = HttpContext.Current;

            // check if session supported
            if (context.Session != null)
            {
                if (context.Session["userId"] == null)
                {
                    context.Response.Redirect("~/Home/FrontPage");
                }
            }
            base.OnActionExecuting(filterContext);
        }
        
       
    }
}