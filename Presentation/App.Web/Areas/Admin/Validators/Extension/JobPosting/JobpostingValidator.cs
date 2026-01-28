using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Departments;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Framework.Validators;
using FluentValidation;

namespace App.Web.Areas.Admin.Validators.Extension.Departments
{
    public partial class JobpostingValidator : BaseNopValidator<JobPostingModel>
    {
        public JobpostingValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Title"));
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Description"));
            RuleFor(model => model.PositionId)
               .NotEmpty()
               .WithMessageAwait(localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Position.select"));
        }
    }
}