using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateTemplate service
    /// </summary>
    public partial class UpdateTemplateService : IUpdateTemplateService
    {
        #region Fields

        private readonly IRepository<UpdateTemplate> _updateTemplateRepository;

        #endregion

        #region Ctor

        public UpdateTemplateService(IRepository<UpdateTemplate> updateTemplateRepository)
        {
            _updateTemplateRepository = updateTemplateRepository;
        }

        #endregion

        #region Methods

        #region UpdateTemplate

        public async Task InsertAsync(UpdateTemplate template)
        {
            await _updateTemplateRepository.InsertAsync(template);
        }

        public async Task UpdateAsync(UpdateTemplate template)
        {
            await _updateTemplateRepository.UpdateAsync(template);
        }

        public async Task DeleteAsync(UpdateTemplate template)
        {
            await _updateTemplateRepository.DeleteAsync(template);
        }
        public virtual async Task<IList<UpdateTemplate>> GetByIdsAsync(int[] updateTemplateIds)
        {
            return await _updateTemplateRepository.GetByIdsAsync(updateTemplateIds);
        }
        public async Task<UpdateTemplate> GetByIdAsync(int id)
        {
            return await _updateTemplateRepository.GetByIdAsync(id);
        }

        public virtual async Task<IPagedList<UpdateTemplate>> GetAllUpdateTemplatesAsync(string title = null, int frequencyId = 0, bool? isActive = null,
    DateTime? dueDate = null,string dueTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = await _updateTemplateRepository.GetAllAsync(async q =>
            {
                if (!string.IsNullOrWhiteSpace(title))
                    q = q.Where(x => x.Title.Contains(title));

                if (frequencyId > 0)
                    q = q.Where(x => x.FrequencyId == frequencyId);

                if (isActive.HasValue)
                    q = q.Where(x => x.IsActive == isActive.Value);

                if (dueDate.HasValue)
                    q = q.Where(x => x.DueDate.HasValue && x.DueDate.Value.Date == dueDate.Value.Date);

                if (!string.IsNullOrWhiteSpace(dueTime))
                {
                    var trimmedDueTime = dueTime.Trim(); // e.g., "15:17"
                    q = q.Where(x => !string.IsNullOrEmpty(x.DueTime) && x.DueTime.StartsWith(trimmedDueTime));
                }

                return q.OrderByDescending(x => x.CreatedOnUTC);
            });

            return new PagedList<UpdateTemplate>(query.ToList(), pageIndex, pageSize);
        }


        #endregion

        #endregion
    }
}