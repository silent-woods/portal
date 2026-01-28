using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        public virtual IActionResult Index()
        {
            return View();
        }
    }
}