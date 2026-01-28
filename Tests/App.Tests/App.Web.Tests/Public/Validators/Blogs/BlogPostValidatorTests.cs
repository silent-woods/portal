using FluentValidation.TestHelper;
using App.Web.Models.Blogs;
using App.Web.Validators.Blogs;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Public.Validators.Blogs
{
    [TestFixture]
    public class BlogPostValidatorTests : BaseNopTest
    {
        private BlogPostValidator _validator;
        
        [OneTimeSetUp]
        public void Setup()
        {
            _validator = GetService<BlogPostValidator>();
        }

        [Test]
        public void ShouldHaveErrorWhenCommentIsNullOrEmpty()
        {
            var model = new BlogPostModel { AddNewComment = { CommentText = null } };
            _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.AddNewComment.CommentText);
            model.AddNewComment.CommentText = string.Empty;
            _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.AddNewComment.CommentText);
        }

        [Test]
        public void ShouldNotHaveErrorWhenCommentIsSpecified()
        {
            var model = new BlogPostModel { AddNewComment = { CommentText = "some comment" } };
            _validator.TestValidate(model).ShouldNotHaveValidationErrorFor(x => x.AddNewComment.CommentText);
        }
    }
}
