using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Affiliates
{
    /// <summary>
    /// Represents an affiliate search model
    /// </summary>
    public partial record AffiliateSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Affiliates.List.SearchFirstName")]
        public string SearchFirstName { get; set; }

        [NopResourceDisplayName("Admin.Affiliates.List.SearchLastName")]
        public string SearchLastName { get; set; }

        [NopResourceDisplayName("Admin.Affiliates.List.SearchFriendlyUrlName")]
        public string SearchFriendlyUrlName { get; set; }

        #endregion
    }
}