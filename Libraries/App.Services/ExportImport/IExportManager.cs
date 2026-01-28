using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Messages;

namespace App.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public partial interface IExportManager
    {
        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<byte[]> ExportCustomersToXlsxAsync(IList<Customer> customers);

        /// <summary>
        /// Export customer list to XML
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result in XML format
        /// </returns>
        Task<string> ExportCustomersToXmlAsync(IList<Customer> customers);
    }
}
