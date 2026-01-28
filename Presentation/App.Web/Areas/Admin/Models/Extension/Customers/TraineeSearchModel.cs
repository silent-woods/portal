using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer search model
    /// </summary>
    public partial record TraineeSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Customers.Candidate.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Candidate.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Customers.Candidate.Fields.MobileNumber")]
        public string MobileNo { get; set; }

        [NopResourceDisplayName("Admin.Customers.Candidate.Fields.Degree")]
        public string Degree { get; set; }
    }
}