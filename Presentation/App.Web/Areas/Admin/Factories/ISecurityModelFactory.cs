using System.Threading.Tasks;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the security model factory
    /// </summary>
    public partial interface ISecurityModelFactory
    {
        /// <summary>
        /// Prepare permission mapping model
        /// </summary>
        /// <param name="model">Permission mapping model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the permission mapping model
        /// </returns>
        Task<PermissionMappingModel> PreparePermissionMappingModelAsync(PermissionMappingModel model);

        #region Permission Record User Mapping Methods

        Task<PermissionUserMappingModel> PreparePermissionUserMappingModelAsync(PermissionUserMappingModel model);

        Task<PermissionUserRecordModel> PreparePermissionUserRecordModelAsync(PermissionUserRecordModel model);

        #endregion
    }
}