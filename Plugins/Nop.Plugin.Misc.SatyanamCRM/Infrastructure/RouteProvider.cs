using App.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;


namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
               name: "Unsubscribe",
               pattern: "unsubscribe",
               defaults: new { controller = "Unsubscribe", action = "Index" });
            
            endpointRouteBuilder.MapControllerRoute(
                name: "Resubscribe",
                pattern: "resubscribe",
                defaults: new { controller = "Unsubscribe", action = "Resubscribe" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}