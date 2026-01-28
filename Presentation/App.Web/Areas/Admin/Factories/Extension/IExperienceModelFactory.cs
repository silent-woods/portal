using App.Core.Domain.Employees;
using App.Web.Areas.Admin.Models.Employees;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Experience model factory
    /// </summary>
    public partial interface IExperienceModelFactory
    {
        Task<ExperienceSearchModel> PrepareExperienceSearchModelAsync(ExperienceSearchModel searchModel);
        Task<ExperienceListModel> PrepareExperienceListModelAsync(ExperienceSearchModel searchModel,Employee employee);

        Task<ExperienceModel> PrepareExperienceModelAsync(ExperienceModel model, Experience experience, bool excludeProperties = false);
    }
}