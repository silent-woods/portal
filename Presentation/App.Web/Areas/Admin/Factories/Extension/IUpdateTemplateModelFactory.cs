using App.Web.Areas.Admin.Models.Extension.UpdateTemplate;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface IUpdateTemplateModelFactory
    {
        Task<UpdateTemplateSearchModel> PrepareUpdateTempleteSearchModelAsync(UpdateTemplateSearchModel searchModel);
        Task<UpdateTemplateListModel> PrepareUpdateTemplateListModelAsync(UpdateTemplateSearchModel searchModel);
        Task<UpdateTemplateModel> PrepareUpdateTemplateModelAsync(UpdateTemplateModel model, UpdateTemplate updateTemplate, bool excludeProperties = false);
    }
}
