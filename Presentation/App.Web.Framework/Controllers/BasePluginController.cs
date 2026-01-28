using App.Web.Framework.Mvc.Filters;

namespace App.Web.Framework.Controllers
{
    /// <summary>
    /// Base controller for plugins
    /// </summary>
    [NotNullValidationMessage]
    public abstract partial class BasePluginController : BaseController
    {
    }
}
