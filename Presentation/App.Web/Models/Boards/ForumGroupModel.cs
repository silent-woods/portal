using System.Collections.Generic;
using App.Web.Framework.Models;

namespace App.Web.Models.Boards
{
    public partial  record ForumGroupModel : BaseNopModel
    {
        public ForumGroupModel()
        {
            Forums = new List<ForumRowModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }

        public IList<ForumRowModel> Forums { get; set; }
    }
}