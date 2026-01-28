namespace App.Web.Areas.Admin.Models.Security
{
    public partial class PermissionUserMappingActionModel
    {
        #region Properties

        public int UserId { get; set; }

        public int PermissionRecordId { get; set; }

        public string PermissionName { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }

        public bool View { get; set; }

        public bool Full { get; set; }

        #endregion
    }
}
