using System;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Cms
{
    public partial record RenderWidgetModel : BaseNopModel
    {
        public Type WidgetViewComponent { get; set; }
        public object WidgetViewComponentArguments { get; set; }
    }
}