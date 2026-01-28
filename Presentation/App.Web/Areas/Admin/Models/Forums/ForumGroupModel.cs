using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Forums
{
    /// <summary>
    /// Represents a forum group model
    /// </summary>
    public partial record ForumGroupModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}