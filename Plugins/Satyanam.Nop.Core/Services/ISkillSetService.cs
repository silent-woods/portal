using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ISkillSet service interface
    /// </summary>
    public partial interface ISkillSetService
    {
        #region SkillSet
        /// <summary>
        /// Get skill by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SkillSet> GetSkillByIdAsync(int id);
        /// <summary>
        /// Get skill by ids
        /// </summary>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        Task<IList<SkillSet>> GetSkillByIdsAsync(int[] skillIds);
        /// <summary>
        /// Get all skill (paged)
        /// </summary>
        /// <param name="technologyId"></param>
        /// <param name="name"></param>
        /// <param name="published"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<SkillSet>> GetAllSkillsAsync(string name = "",bool? published = null,int pageIndex = 0,int pageSize = int.MaxValue);
        /// <summary>
        /// Insert skill
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        Task InsertSkillAsync(SkillSet skill);
        /// <summary>
        /// Update skill
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        Task UpdateSkillAsync(SkillSet skill);
        /// <summary>
        /// Delete skill
        /// </summary>  
        Task DeleteSkillAsync(SkillSet skill);
        #endregion
    }
}