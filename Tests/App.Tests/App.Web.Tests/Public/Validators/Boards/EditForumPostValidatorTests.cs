using FluentValidation.TestHelper;
using App.Web.Models.Boards;
using App.Web.Validators.Boards;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Public.Validators.Boards
{
    [TestFixture]
    public class EditForumPostValidatorTests : BaseNopTest
    {
        private EditForumPostValidator _validator;
        
        [OneTimeSetUp]
        public void Setup()
        {
            _validator = GetService<EditForumPostValidator>();
        }
        
        [Test]
        public void ShouldHaveErrorWhenTextIsNullOrEmpty()
        {
            var model = new EditForumPostModel
            {
                Text = null
            };
            _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Text);
            model.Text = string.Empty;
            _validator.TestValidate(model).ShouldHaveValidationErrorFor(x => x.Text);
        }

        [Test]
        public void ShouldNotHaveErrorWhenTextIsSpecified()
        {
            var model = new EditForumPostModel
            {
                Text = "some comment"
            };
            _validator.TestValidate(model).ShouldNotHaveValidationErrorFor(x => x.Text);
        }
    }
}
