using System.Threading;
using System.Web;
using System.Web.Optimization;

namespace OwnBI
{
    public class BundleConfig
    {
        // Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=301862"
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/chartjs").Include(
                        "~/Scripts/Chart.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqcloud").Include(
                        "~/Scripts/jqcloud.js"));

            bundles.Add(new ScriptBundle("~/bundles/sortable").Include(
                        "~/Scripts/Sortable.js"));


            // Verwenden Sie die Entwicklungsversion von Modernizr zum Entwickeln und Erweitern Ihrer Kenntnisse. Wenn Sie dann
            // für die Produktion bereit sind, verwenden Sie das Buildtool unter "http://modernizr.com", um nur die benötigten Tests auszuwählen.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/moment.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-datetimepicker.js",
					  "~/Scripts/bootstrap3-typeahead.min.js"
                      )
            );

            bundles.Add(new ScriptBundle("~/bundles/textcomplete").Include(
                "~/Scripts/jquery.textcomplete.js"
                )
            ); ;

           

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-datetimepicker.css",
                      "~/Content/site.css",
                      "~/Content/jqcloud.css"));

            // Festlegen von "EnableOptimizations" auf "false" für Debugzwecke. Weitere Informationen
            // finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=301862".
            BundleTable.EnableOptimizations = true;
        }
    }
}
