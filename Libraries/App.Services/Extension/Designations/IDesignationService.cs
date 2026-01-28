using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Designations;

namespace App.Services.Designations
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface IDesignationService
    {
        /// <summary>
        /// Gets all Designation
        /// </summary>
        Task<IPagedList<Designation>> GetAllDesignationAsync(string designationName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get designation by id
        /// </summary>
        /// <param name="designationId"></param>
        /// <returns></returns>
        Task<Designation> GetDesignationByIdAsync(int designationId);

        /// <summary>
        /// Insert designation
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        Task InsertDesignationAsync(Designation designation);

        /// <summary>
        /// Update designation
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        Task UpdateDesignationAsync(Designation designation);

        /// <summary>
        /// Delete designation
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        Task DeleteDesignationAsync(Designation designation);

        /// <summary>
        /// Get designation by ids
        /// </summary>
        /// <param name="designationIds"></param>
        /// <returns></returns>
        Task<IList<Designation>> GetDesignationByIdsAsync(int[] designationIds);


      Task<IList<Designation>> GetCanRatingDesignationAsync();

        Task<int> GetProjectLeaderRoleId();

        Task<int> GetRoleIdProjectManager();

        Task<int> GetQARoleId();

        Task<int> GetHrRoleId();

        Task<IList<Designation>> GetDesignationsByIdsAsync(int[] ids);
    }
}