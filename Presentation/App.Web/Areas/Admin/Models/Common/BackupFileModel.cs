using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents a backup file model
    /// </summary>
    public partial record BackupFileModel : BaseNopModel
    {
        #region Properties
        
        public string Name { get; set; }
        
        public string Length { get; set; }
        
        public string Link { get; set; }
        
        #endregion
    }
}