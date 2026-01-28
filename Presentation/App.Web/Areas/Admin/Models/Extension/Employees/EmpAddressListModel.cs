using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a Address list model
    /// </summary>
    public partial record EmpAddressListModel : BasePagedListModel<EmpAddressModel>
    {
      
    }
}