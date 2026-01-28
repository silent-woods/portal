using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Extension.CheckLists
{
    /// <summary>
    /// Represents a CheckList Master search model
    /// </summary>
    public partial record CheckListMasterSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.CheckLists.Fields.SearchTitle")]
        public string SearchTitle { get; set; }
    }
}
