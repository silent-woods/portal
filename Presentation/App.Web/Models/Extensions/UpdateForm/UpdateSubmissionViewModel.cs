using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record UpdateSubmissionViewModel : BaseNopEntityModel
    {
        public UpdateSubmissionViewModel()
        {
            Comments = new List<UpdateSubmissionCommentModel>();
        }

        #region Properties

        public List<UpdateSubmissionCommentModel> Comments { get; set; }
        #endregion
    }




}
