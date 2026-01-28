using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.News;

namespace App.Web.Validators.News
{
    public partial class NewsItemValidator : BaseNopValidator<NewsItemModel>
    {
        public NewsItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AddNewComment.CommentTitle).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("News.Comments.CommentTitle.Required")).When(x => x.AddNewComment != null);
            RuleFor(x => x.AddNewComment.CommentTitle).Length(1, 200).WithMessageAwait(localizationService.GetResourceAsync("News.Comments.CommentTitle.MaxLengthValidation"), 200).When(x => x.AddNewComment != null && !string.IsNullOrEmpty(x.AddNewComment.CommentTitle));
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("News.Comments.CommentText.Required")).When(x => x.AddNewComment != null);
        }
    }
}