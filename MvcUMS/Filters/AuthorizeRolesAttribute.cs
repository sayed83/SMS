using MvcEnergyPac.Models;
using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MvcUMS.Filters
{
    public class AuthorizeRolesAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private readonly string[] userAssignedRoles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            this.userAssignedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            using (UsersContext db = new UsersContext())
            {
                UserManager usrMngr = new UserManager();
                foreach(var roles in userAssignedRoles)
                {
                    authorize = usrMngr.isUserInRole(httpContext.User.Identity.Name, roles);
                    if (authorize)
                        return authorize;
                }
            }

            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/notAuthorize");
        }
    }
}