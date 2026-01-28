using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a message template search model
    /// </summary>
    public partial record MessageTemplateSearchModel : BaseSearchModel
    {
        #region Ctor

        public MessageTemplateSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableActiveOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.SearchKeywords")]
        public string SearchKeywords { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.IsActive")]
        public int IsActiveId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableActiveOptions { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}