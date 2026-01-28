using System.Collections.Generic;
using App.Web.Framework.Models;

namespace App.Web.Models.Boards
{
    public partial record BoardsIndexModel : BaseNopModel
    {
        public BoardsIndexModel()
        {
            ForumGroups = new List<ForumGroupModel>();
        }
        
        public IList<ForumGroupModel> ForumGroups { get; set; }
    }
}