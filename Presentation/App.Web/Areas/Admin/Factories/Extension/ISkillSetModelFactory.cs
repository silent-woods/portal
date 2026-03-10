using App.Web.Areas.Admin.Models.Extension.SkillSet;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ISkillSetModelFactory
    {
        Task<SkillSetSearchModel> PrepareSkillSetSearchModelAsync(SkillSetSearchModel searchModel);
        Task<SkillSetListModel> PrepareSkillSetListModelAsync(SkillSetSearchModel searchModel); 
        Task<SkillSetModel> PrepareSkillSetModelAsync(SkillSetModel model, SkillSet entity, bool excludeProperties = false);

    }
}
