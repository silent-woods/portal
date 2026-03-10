using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ITechnologySkillMapping service interface
    /// </summary>
    public partial interface ITechnologySkillMappingService
    {
        #region TechnologySkillMapping
        Task<IList<TechnologySkillMapping>> GetBySkillSetIdAsync(int skillSetId);
        Task<IList<TechnologySkillMapping>> GetByTechnologyIdAsync(int technologyId);
        Task InsertAsync(TechnologySkillMapping mapping);
        Task DeleteAsync(TechnologySkillMapping mapping);
        Task DeleteBySkillSetIdAsync(int skillSetId);
        #endregion
    }
}