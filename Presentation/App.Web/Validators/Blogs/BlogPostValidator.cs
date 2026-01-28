using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Blogs;

namespace App.Web.Validators.Blogs
{
    public partial class BlogPostValidator : BaseNopValidator<BlogPostModel>
    {
        public BlogPostValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AddNewComment.CommentText).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Blog.Comments.CommentText.Required")).When(x => x.AddNewComment != null);
        }
    }
}