using FluentValidation;
using App.Core.Domain.Polls;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Polls;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Polls
{
    public partial class PollValidator : BaseNopValidator<PollModel>
    {
        public PollValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.Polls.Fields.Name.Required"));

            SetDatabaseValidationRules<Poll>(mappingEntityAccessor);
        }
    }
}