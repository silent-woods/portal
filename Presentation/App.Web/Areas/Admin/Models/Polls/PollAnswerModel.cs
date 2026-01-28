using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Polls
{
    /// <summary>
    /// Represents a poll answer model
    /// </summary>
    public partial record PollAnswerModel : BaseNopEntityModel
    {
        #region Properties

        public int PollId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.NumberOfVotes")]
        public int NumberOfVotes { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Polls.Answers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}