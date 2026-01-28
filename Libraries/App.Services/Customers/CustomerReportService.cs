using System;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Customers;
using App.Data;
using App.Data.Extensions;
using App.Services.Helpers;

namespace App.Services.Customers
{
    /// <summary>
    /// Customer report service
    /// </summary>
    public partial class CustomerReportService : ICustomerReportService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<Customer> _customerRepository;
     
        #endregion

        #region Ctor

        public CustomerReportService(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IRepository<Customer> customerRepository)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _customerRepository = customerRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get best customers
        /// </summary>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shipment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the report
        /// </returns>
        public virtual async Task<IPagedList<BestCustomerReportLine>> GetBestCustomersReportAsync(DateTime? createdFromUtc,
            DateTime? createdToUtc, 
            int pageIndex = 0, int pageSize = 214748364)
        {
            
            var query1 = from c in _customerRepository.Table
                         where (!createdFromUtc.HasValue) &&
                         (!createdToUtc.HasValue) &&
                         !c.Deleted
                         select new { c };

            var query2 = from co in query1
                         group co by co.c.Id into g
                         select new
                         {
                             CustomerId = g.Key,
                         };
           
            var tmp = await query2.ToPagedListAsync(pageIndex, pageSize);
            return new PagedList<BestCustomerReportLine>(tmp.Select(x => new BestCustomerReportLine
            {
                CustomerId = x.CustomerId,
            }).ToList(),
                tmp.PageIndex, tmp.PageSize, tmp.TotalCount);
        }

        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of registered customers
        /// </returns>
        public virtual async Task<int> GetRegisteredCustomersReportAsync(int days)
        {
            var date = (await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now)).AddDays(-days);

            var registeredCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredCustomerRole == null)
                return 0;

            return (await _customerService.GetAllCustomersAsync(
                date,
                customerRoleIds: new[] { registeredCustomerRole.Id })).Count;
        }

        #endregion
    }
}