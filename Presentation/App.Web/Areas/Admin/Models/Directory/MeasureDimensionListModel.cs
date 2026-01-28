using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a measure dimension list model
    /// </summary>
    public partial record MeasureDimensionListModel : BasePagedListModel<MeasureDimensionModel>
    {
    }
}