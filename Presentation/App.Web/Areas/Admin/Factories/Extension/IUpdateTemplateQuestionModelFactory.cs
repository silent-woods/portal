using App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface IUpdateTemplateQuestionModelFactory
    {
        Task<UpdateTemplateQuestionSearchModel> PrepareUpdateTempleteQuestionSearchModelAsync(UpdateTemplateQuestionSearchModel searchModel);
        Task<UpdateTemplateQuestionListModel> PrepareUpdateTemplateQuestionListModelAsync(UpdateTemplateQuestionSearchModel searchModel);
        Task<UpdateTemplateQuestionModel> PrepareUpdateTemplateQuestionModelAsync(UpdateTemplateQuestionModel model, UpdateTemplateQuestion updateTemplateQuestion, bool excludeProperties = false);
    }
}
