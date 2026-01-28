using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Inquiry service
    /// </summary>
    public partial class InquiryService : IInquiryService
    {
        #region Fields

        private readonly IRepository<Inquiry> _inquiryRepository;

        #endregion

        #region Ctor

        public InquiryService(IRepository<Inquiry> inquiryRepository)
        {
            _inquiryRepository = inquiryRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all Inquiry
        /// </summary>
        public virtual async Task<IPagedList<Inquiry>> GetAllInquiryAsync(
      string name = null,
      string email = null,
      string contactNo = null,
      string company = null,
      int? sourceId = null,
      int pageIndex = 0,
      int pageSize = int.MaxValue)
        {
            var query = _inquiryRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                var trimmedName = name.Trim().ToLower();
                query = query.Where(f =>
                    (f.FirstName != null && f.FirstName.ToLower().Contains(trimmedName)) ||
                    (f.LastName != null && f.LastName.ToLower().Contains(trimmedName)));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var trimmedEmail = email.Trim().ToLower();
                query = query.Where(f => f.Email != null && f.Email.ToLower().Contains(trimmedEmail));
            }

            if (!string.IsNullOrWhiteSpace(contactNo))
            {
                var trimmedContact = contactNo.Trim();
                query = query.Where(f => f.ContactNo != null && f.ContactNo.Contains(trimmedContact));
            }

            if (!string.IsNullOrWhiteSpace(company))
            {
                var trimmedCompany = company.Trim().ToLower();
                query = query.Where(f => f.Company != null && f.Company.ToLower().Contains(trimmedCompany));
            }

            if (sourceId.HasValue && sourceId.Value > 0)
            {
                query = query.Where(f => f.SourceId == sourceId.Value);
            }

            query = query.OrderByDescending(f => f.CreatedOnUtc);

            var pagedList = new PagedList<Inquiry>(query.ToList(), pageIndex, pageSize);

            return await Task.FromResult(pagedList);
        }


        /// <summary>
        /// Gets a Inquiry by Id
        /// </summary>
        public virtual async Task<Inquiry> GetInquiryByIdAsync(int id)
        {
            return await _inquiryRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<Inquiry>> GetInquiriesByIdsAsync(int[] inquiryIds)
        {
            if (inquiryIds == null || inquiryIds.Length == 0)
                return new List<Inquiry>();

            var query = _inquiryRepository.Table.Where(i => inquiryIds.Contains(i.Id));

            var inquiries = query.ToList()
                .OrderBy(i => Array.IndexOf(inquiryIds, i.Id))
                .ToList();

            return await Task.FromResult(inquiries);
        }


        /// <summary>
        /// Inserts a new Inquiry
        /// </summary>
        public virtual async Task InsertInquiryAsync(Inquiry form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            form.CreatedOnUtc = DateTime.UtcNow;

            await _inquiryRepository.InsertAsync(form);
        }

        /// <summary>
        /// Updates an Inquiry
        /// </summary>
        public virtual async Task UpdateInquiryAsync(Inquiry form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            await _inquiryRepository.UpdateAsync(form);
        }

        /// <summary>
        /// Deletes a Inquiry
        /// </summary>
        public virtual async Task DeleteInquiryAsync(Inquiry form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            await _inquiryRepository.DeleteAsync(form);
        }

        #endregion
    }
}
