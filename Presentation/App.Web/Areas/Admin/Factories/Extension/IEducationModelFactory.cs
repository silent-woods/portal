using App.Core.Domain.Employees;
using App.Web.Areas.Admin.Models.Employees;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial interface IEducationModelFactory
    {
        Task<EducationSearchModel> PrepareEducationSearchModelAsync(EducationSearchModel searchModel);
        Task<EducationListModel> PrepareEducationListModelAsync(EducationSearchModel searchModel,Employee employee);

        Task<EducationModel> PrepareEducationModelAsync(EducationModel model, Education education, bool excludeProperties = false);
    }
}