using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.CheckListMappings
{
    /// <summary>
    /// Represents a checklist mapping search model
    /// </summary>
    public partial record CheckListMappingSearchModel : BaseSearchModel
    {
        public int TaskCategoryId { get; set; }
        public int StatusId { get; set; }
        public int CheckListId { get; set; }
    }
}
