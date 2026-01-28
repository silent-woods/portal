using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Data;
using LinqToDB;

namespace App.Services.Employees
{
    /// <summary>
    /// Assets service
    /// </summary>
    public partial class EmpAddressService : IEmpAddressService
    {
        #region Fields

        private readonly IRepository<EmpAddress> _addressRepository;

        #endregion

        #region Ctor

        public EmpAddressService(
            IRepository<EmpAddress> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        #endregion

        #region Methods

        #region Address

        /// <summary>
        /// Deletes a Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAddressAsync(EmpAddress address)
        {
            await _addressRepository.DeleteAsync(address);
        }

        /// <summary>
        /// Gets a Address
        /// </summary>
        /// <param name="assestsId">addres identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address
        /// </returns>
        public virtual async Task<EmpAddress> GetAddressByIdAsync(int addressId)
        {
            return await _addressRepository.GetByIdAsync(addressId, cache => default);
        }

        /// <summary>
        /// Gets all address
        /// </summary>
        /// <param name="addressName">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="address">Filter by address name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address
        /// </returns>

        public virtual async Task<IPagedList<EmpAddress>> GetAllAddressAsync(int employeeId,string addressName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            return await _addressRepository.GetAllPagedAsync(async query =>
            {
                if (employeeId != 0)
                {
                    query = query.Where(b => b.EmployeeID == employeeId);
                }
                
                return query.OrderByDescending(c => c.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAddressAsync(EmpAddress address)
        {
            await _addressRepository.InsertAsync(address);
        }

        /// <summary>
        /// Updates the Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAddressAsync(EmpAddress address)
        {
            await _addressRepository.UpdateAsync(address);
        }

        public virtual async Task<IList<EmpAddress>> GetAddressByIdsAsync(int[] addressIds)
        {
            return await _addressRepository.GetByIdsAsync(addressIds, cache => default, false);
        }


        public async Task<IEnumerable<EmpAddress>> GetAllAddressAsync(int employeeId, string keyword)
        {
            var query = _addressRepository.Table.Where(a => a.EmployeeID == employeeId);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Address1.Contains(keyword) || a.Address2.Contains(keyword));
            }

            return await query.ToListAsync();
        }

        #endregion

        #endregion
    }
}