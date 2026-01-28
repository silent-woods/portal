using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    public partial record SystemWarningModel : BaseNopModel
    {
        public SystemWarningLevel Level { get; set; }

        public string Text { get; set; }

        public bool DontEncode { get; set; }
    }
}