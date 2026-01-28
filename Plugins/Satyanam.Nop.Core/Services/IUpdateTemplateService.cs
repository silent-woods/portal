using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateTemplate service interface
    /// </summary>
    public partial interface IUpdateTemplateService
    {
        Task InsertAsync(UpdateTemplate template);
        Task UpdateAsync(UpdateTemplate template);
        Task DeleteAsync(UpdateTemplate template);
        Task<IList<UpdateTemplate>> GetByIdsAsync(int[] updateTemplateIds);
        Task<UpdateTemplate> GetByIdAsync(int id);
        Task<IPagedList<UpdateTemplate>> GetAllUpdateTemplatesAsync(string title = null, int frequencyId = 0, bool? isActive = null,
        DateTime? dueDate = null, string dueTime = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}