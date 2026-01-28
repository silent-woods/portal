using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Designations;
using App.Data;
using App.Data.Extensions;

namespace App.Services.Designations
{
    /// <summary>
    /// Student service
    /// </summary>
    public partial class DesignationService : IDesignationService
    {
        #region Fields

        private readonly IRepository<Designation> _designationRepository;

        #endregion

        #region Ctor

        public DesignationService(IRepository<Designation> designationRepository
           )
        {
            _designationRepository = designationRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all designations
        /// </summary>
        /// <param name="designationName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Designation>> GetAllDesignationAsync(string designationName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _designationRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(designationName))
                    query = query.Where(c => c.Name.Contains(designationName));

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Designation>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<IList<Designation>> GetCanRatingDesignationAsync()
        {
            var allDesignations = await _designationRepository.GetAllAsync(
                query=> query.Where(d => d.CanGiveRatings)
                .OrderByDescending(d => d.CreateOnUtc)
     
                );

            
            return allDesignations;
        }
        /// <summary>
        /// Get designation by id
        /// </summary>
        /// <param name="designationId"></param>
        /// <returns></returns>
        /// 
        public virtual async Task<Designation> GetDesignationByIdAsync(int designationId)
        {
            return await _designationRepository.GetByIdAsync(designationId, cache => default);
        }

        /// <summary>
        /// Get designation by ids
        /// </summary>
        /// <param name="designationIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<Designation>> GetDesignationByIdsAsync(int[] designationIds)
        {
            return await _designationRepository.GetByIdsAsync(designationIds, cache => default, false);
        }

        /// <summary>
        /// Insert designation
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        public virtual async Task InsertDesignationAsync(Designation designation)
        {
            await _designationRepository.InsertAsync(designation);
        }

        /// <summary>
        /// Update designation
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateDesignationAsync(Designation designation)
        {
            if (designation == null)
                throw new ArgumentNullException(nameof(designation));

            await _designationRepository.UpdateAsync(designation);
        }

        /// <summary>
        /// delete designation by record
        /// </summary>
        /// <param name="designation"></param>
        /// <returns></returns>
        public virtual async Task DeleteDesignationAsync(Designation designation)
        {
            await _designationRepository.DeleteAsync(designation, false);
        }




        public virtual async Task<int> GetProjectLeaderRoleId()
        {

            int projectLeaderRoleId = 0;
            var designations = await GetAllDesignationAsync("");
            foreach (var designation in designations)
            {
                if (designation.Name.ToLower().Trim() == "team leader" || designation.Name.ToLower().Trim() == "project leader")
                    projectLeaderRoleId=designation.Id;

            }
            return projectLeaderRoleId;
        }
        public virtual async Task<int> GetHrRoleId()
        {

            int projectLeaderRoleId = 0;
            var designations = await GetAllDesignationAsync("");
            foreach (var designation in designations)
            {
                if (designation.Name.ToLower().Trim() == "human resource" || designation.Name.ToLower().Trim() == "hr")
                    projectLeaderRoleId = designation.Id;

            }
            return projectLeaderRoleId;
        }
        public virtual async Task<int> GetRoleIdProjectManager()
        {

            int projectManagerRoleId = 0;
            var designations = await GetAllDesignationAsync("");
            foreach (var designation in designations)
            {
                if (designation.Name.ToLower().Trim() == "manager" || designation.Name.ToLower().Trim() == "project manager" || designation.Name.ToLower().Trim() == "team manager")
                    projectManagerRoleId=designation.Id;

            }
            return projectManagerRoleId;
        }

        public virtual async Task<int> GetQARoleId()
        {

            int qaRoleId = 0;
            var designations = await GetAllDesignationAsync("");
            foreach (var designation in designations)
            {
                if (designation.Name.ToLower().Trim() == "qa" || designation.Name.ToLower().Trim() == "tester" || designation.Name.ToLower().Trim() == "q.a")
                    qaRoleId = designation.Id;

            }
            return qaRoleId;
        }

        public async Task<IList<Designation>> GetDesignationsByIdsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<Designation>();

            var query = _designationRepository.Table.Where(d => ids.Contains(d.Id));
            return await query.ToListAsync();
        }

        #endregion
    }
}