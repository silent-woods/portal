using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Satyanam.Plugin.Misc.Emailverification.Infrastructure
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
                name: "SubmitDiscussProjectForm",
                pattern: $"Email/SubmitDiscussProjectForm",
                defaults: new { controller = "EmailCommon", action = "SubmitDiscussProjectForm" });

     //       endpointRouteBuilder.MapControllerRoute(
     //name: "ContactUs",
     //pattern: "Common/contactus",
     //defaults: new { controller = "EmailCommon", action = "ContactUsSend" });

        }


        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}