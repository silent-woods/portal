using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class PrivateMessagesSentItemsViewComponent : NopViewComponent
    {
        private readonly IPrivateMessagesModelFactory _privateMessagesModelFactory;

        public PrivateMessagesSentItemsViewComponent(IPrivateMessagesModelFactory privateMessagesModelFactory)
        {
            _privateMessagesModelFactory = privateMessagesModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int pageNumber, string tab)
        {
            var model = await _privateMessagesModelFactory.PrepareSentModelAsync(pageNumber, tab);
            return View(model);
        }
    }
}
