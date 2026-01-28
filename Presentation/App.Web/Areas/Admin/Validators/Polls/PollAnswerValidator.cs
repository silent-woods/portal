using FluentValidation;
using App.Core.Domain.Polls;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Polls;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Polls
{
    public partial class PollAnswerValidator : BaseNopValidator<PollAnswerModel>
    {
        public PollAnswerValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            //if validation without this set rule is applied, in this case nothing will be validated
            //it's used to prevent auto-validation of child models
            RuleSet(NopValidationDefaults.ValidationRuleSet, () =>
            {
                RuleFor(model => model.Name)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.Polls.Answers.Fields.Name.Required"));

                SetDatabaseValidationRules<PollAnswer>(mappingEntityAccessor);
            });
        }
    }
}