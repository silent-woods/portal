using System.Collections.Generic;
using App.Web.Framework.Models;

namespace App.Web.Models.News
{
    public partial record HomepageNewsItemsModel : BaseNopModel
    {
        public HomepageNewsItemsModel()
        {
            NewsItems = new List<NewsItemModel>();
        }

        public int WorkingLanguageId { get; set; }
        public IList<NewsItemModel> NewsItems { get; set; }
    }
}