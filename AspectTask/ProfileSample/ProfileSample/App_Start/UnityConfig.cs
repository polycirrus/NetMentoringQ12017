using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using ProfileSample.Business;
using Unity.Mvc5;

namespace ProfileSample
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();
            container.AddNewExtension<Interception>();
            container.RegisterType<IImageManager, ImageManager>(
                new Interceptor<InterfaceInterceptor>(),
                new InterceptionBehavior<LoggingInterceptor>()
            );

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}