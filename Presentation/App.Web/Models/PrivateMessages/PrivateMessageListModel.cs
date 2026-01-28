using System.Collections.Generic;
using App.Web.Framework.Models;
using App.Web.Models.Common;

namespace App.Web.Models.PrivateMessages
{
    public partial record PrivateMessageListModel : BaseNopModel
    {
        public IList<PrivateMessageModel> Messages { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}