using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class TopMenuViewComponent : NopViewComponent
    {
  
        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            return View();
        }
    }
}
