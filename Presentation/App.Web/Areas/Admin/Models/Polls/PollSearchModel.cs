using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Polls
{
    /// <summary>
    /// Represents a poll search model
    /// </summary>
    public partial record PollSearchModel : BaseSearchModel
    {
        #region Ctor

        public PollSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.Polls.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}