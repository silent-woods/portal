using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface IInquiryService
    {
        Task<IPagedList<Inquiry>> GetAllInquiryAsync(
              string name = null,
              string email = null,
              string contactNo = null,
              string company = null,
              int? sourceId = null,
              int pageIndex = 0,
              int pageSize = int.MaxValue);
        Task<Inquiry> GetInquiryByIdAsync(int id);
        Task<IList<Inquiry>> GetInquiriesByIdsAsync(int[] inquiryIds);
        Task InsertInquiryAsync(Inquiry form);
        Task UpdateInquiryAsync(Inquiry form);
        Task DeleteInquiryAsync(Inquiry form);
    }
}
