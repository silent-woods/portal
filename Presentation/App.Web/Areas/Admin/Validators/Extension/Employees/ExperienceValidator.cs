using App.Services.Localization;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Validators;

using App.Web.Models.Employee;
using FluentValidation;
using Nop.Data.Mapping;

using System;
using System.Threading.Tasks;
using App.Core.Domain.Employees;
using App.Data.Mapping;

namespace App.Web.Areas.Admin.Validators.Extension.Employees
{
    public partial class ExperienceValidator : BaseNopValidator<ExperienceModel>
    {
        public ExperienceValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.From)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Extension.Department.Fields.Name.Required"));

            RuleFor(x => x.To)
                .Must((model, to) => to > model.From)
                .WithMessage("To Date must be greater than From Date.");

            SetDatabaseValidationRules<Experience>(mappingEntityAccessor);
        }
    }
}
