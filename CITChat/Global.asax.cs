using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using CITChat.App_Start;
using CITChat.Controllers;
using CITChat.Translators;

namespace CITChat
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ConfigureTranslator();
            // Ask Entity Framework to drop and regenerate the database automatically whenever the model schema changes
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CITChatContext>());
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        private static void ConfigureTranslator()
        {
            var containerBuilder = new ContainerBuilder();
            // Register individual components
            containerBuilder.RegisterInstance(new InflationaryEnglishTranslator())
                            .As<IContentTranslator>();
            ContainerManager.Container = containerBuilder.Build();
        }
    }
}