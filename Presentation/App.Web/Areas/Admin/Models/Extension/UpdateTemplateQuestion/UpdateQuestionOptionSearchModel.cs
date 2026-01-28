using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion
{
    public partial record UpdateQuestionOptionSearchModel : BaseSearchModel
    {
        public UpdateQuestionOptionSearchModel()
        {

        }
        #region Properties
        public int UpdateTemplateQuestionId { get; set; }
        #endregion
    }
}
