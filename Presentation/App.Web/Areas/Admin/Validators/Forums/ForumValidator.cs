using FluentValidation;
using App.Core.Domain.Forums;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Forums;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Forums
{
    public partial class ForumValidator : BaseNopValidator<ForumModel>
    {
        public ForumValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.Forums.Forum.Fields.Name.Required"));
            RuleFor(x => x.ForumGroupId).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required"));

            SetDatabaseValidationRules<Forum>(mappingEntityAccessor);
        }
    }
}