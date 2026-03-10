using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Extension.Technologys
{
    public partial record TechnologySearchModel : BaseSearchModel
    {
        public TechnologySearchModel()
        {

        }
        #region Properties
         
        [NopResourceDisplayName("Admin.TechnologySearchModel.Fields.SearchName")]
        public string SearchName { get; set; }
        #endregion
    }
}
