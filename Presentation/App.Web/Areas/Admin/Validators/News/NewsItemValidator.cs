using FluentValidation;
using App.Core.Domain.News;
using App.Data.Mapping;
using App.Services.Localization;
using App.Services.Seo;
using App.Web.Areas.Admin.Models.News;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.News
{
    public partial class NewsItemValidator : BaseNopValidator<NewsItemModel>
    {
        public NewsItemValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Fields.Title.Required"));

            RuleFor(x => x.Short).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Fields.Short.Required"));

            RuleFor(x => x.Full).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems.Fields.Full.Required"));

            RuleFor(x => x.SeName).Length(0, NopSeoDefaults.SearchEngineNameLength)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.SEO.SeName.MaxLengthValidation"), NopSeoDefaults.SearchEngineNameLength);

            SetDatabaseValidationRules<NewsItem>(mappingEntityAccessor);
        }
    }
}