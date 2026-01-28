using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Departments;
using App.Web.Framework.Validators;
using FluentValidation;

namespace App.Web.Areas.Admin.Validators.Extension.Departments
{
    public partial class DepartmentValidator : BaseNopValidator<DepartmentModel>
    {
        public DepartmentValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Extension.Department.Fields.Name.Required"));

        }
    }
}