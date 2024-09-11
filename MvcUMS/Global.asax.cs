using MvcUMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MvcUMS
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            //GlobalConfiguration.Configuration.Filters.Add(new System.Web.Http.AuthorizeAttribute());
            Database.SetInitializer<StuffContext>(null);
            Database.SetInitializer<CountryContex>(null);
            Database.SetInitializer<CityContex>(null);
            Database.SetInitializer<ZIPContext>(null);
            Database.SetInitializer<StudentContext>(null);
            Database.SetInitializer<PaymentContext>(null);
            
           
        }
    }
}