using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.ManageResumes
{
    /// <summary>
    /// Represents a timesheet list model
    /// </summary>candiatesResumesModel
    public partial record CandiatesListModel : BaseNopModel
    {
        public CandiatesListModel()
        {
            Addresses = new List<CandiatesResumesModel>();
        }

        public IList<CandiatesResumesModel> Addresses { get; set; }
    }
}