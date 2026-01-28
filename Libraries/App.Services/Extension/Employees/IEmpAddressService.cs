using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;

namespace App.Services.Employees
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IEmpAddressService
    {
        /// <summary>
        /// Deletes a Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAddressAsync(EmpAddress address);

        /// <summary>
        /// Gets a Address
        /// </summary>
        /// <param name="addressId">address identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address
        /// </returns>
        Task<EmpAddress> GetAddressByIdAsync(int addressId);

        /// <summary>
        /// Gets all Address
        /// </summary>
        /// <param name="addressName">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="assestsName">Filter by address name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address
        /// </returns>

        Task<IPagedList<EmpAddress>> GetAllAddressAsync(int employeeId,string addressName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Inserts a address
        /// </summary>
        /// <param name="experience">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAddressAsync(EmpAddress address);

        /// <summary>
        /// Updates the Address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAddressAsync(EmpAddress address);

        Task<IList<EmpAddress>> GetAddressByIdsAsync(int[] addressIds);
        Task<IEnumerable<EmpAddress>> GetAllAddressAsync(int employeeId, string keyword);
    }
}