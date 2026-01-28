using System.Threading.Tasks;
using App.Core.Domain.Common;
using App.Web.Areas.Admin.Models.Common;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the address model factory
    /// </summary>
    public partial interface IAddressModelFactory
    {
        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareAddressModelAsync(AddressModel model, Address address = null);
    }
}