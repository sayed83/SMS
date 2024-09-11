    using System.Web;
using System.Web.Optimization;

namespace MvcUMS
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {


            bundles.IgnoreList.Clear();
            System.Web.Optimization.BundleTable.EnableOptimizations = true;

            bundles.Add(new ScriptBundle("~/scripts/core").Include(
                    "~/Content/dashboard_template/assets/js/plugins/loaders/pace.min.js",
                    "~/Content/dashboard_template/assets/js/core/libraries/jquery.min.js",
                    "~/Content/dashboard_template/assets/js/core/libraries/bootstrap.min.js",
                    "~/Content/dashboard_template/assets/js/plugins/loaders/blockui.min.js"
                ));


            bundles.Add(new ScriptBundle("~/scripts/themeJs").Include(
                 "~/Content/dashboard_template/assets/js/plugins/forms/styling/switchery.min.js",
                 "~/Content/dashboard_template/assets/js/plugins/forms/styling/uniform.min.js",
                   "~/Content/dashboard_template/assets/js/plugins/ui/moment/moment.min.js",
                    "~/Content/dashboard_template/assets/js/plugins/pickers/daterangepicker.js",
                 "~/Content/dashboard_template/assets/js/plugins/ui/fullcalendar/fullcalendar.min.js",
                  "~/Content/dashboard_template/assets/js/core/libraries/jquery_ui/widgets.min.js",
                 "~/Content/dashboard_template/assets/js/plugins/tables/datatables/datatables.min.js",
                   "~/Content/dashboard_template/assets/js/plugins/forms/selects/select2.min.js",
                  "~/Content/dashboard_template/assets/js/plugins/tables/extensions/buttons.min.js",
                 "~/Content/dashboard_template/assets/js/plugins/bootstrap-toastr/toastr.min.js",
                 "~/Content/dashboard_template/assets/js/plugins/forms/selects/bootstrap_multiselect.js",

                 "~/Content/dashboard_template/assets/js/core/app.js",
                 "~/Content/dashboard_template/assets/js/pages/form_layouts.js",
                 "~/Content/dashboard_template/assets/js/pages/jqueryui_forms.js",
                 "~/Content/dashboard_template/assets/js/pages/datatables_basic.js",
                 "~/Content/dashboard_template/assets/js/pages/datatables_extension_buttons_print.js",
                 "~/Content/dashboard_template/assets/js/pages/components_modals.js",
                 "~/Scripts/printThis.js",
                 "~/Scripts/knockout-{version}.js",
                 "~/Scripts/knockout.namepathbinding.js",
                 "~/Scripts/year-select.js"
               ));


            bundles.Add(new ScriptBundle("~/bundles/partialJs").Include(
                        "~/Content/dashboard_template/assets/js/pages/datatables_basic.js"));
            

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));


            bundles.Add(new StyleBundle("~/Content/globalCss").Include(
                    "~/Content/dashboard_template/assets/css/icons/icomoon/styles.css",
                     "~/Content/dashboard_template/assets/css/bootstrap.css",
                      "~/Content/dashboard_template/assets/css/core.css",
                       "~/Content/dashboard_template/assets/css/components.css",
                        "~/Content/dashboard_template/assets/css/colors.css"
                ));

            bundles.Add(new StyleBundle("~/Content/themeCss").Include(
                       "~/Content/dashboard_template/assets/js/plugins/bootstrap-toastr/toastr.min.css",
                       "~/Content/Custom.css"
               ));
        }
    }

    public class BundlesFormats
    {
        public const string PRINT = @"<link href=""{0}"" rel=""stylesheet"" type=""text/css"" media=""screen,print"" />";
    }
}