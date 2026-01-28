using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// AccountType service interface
    /// </summary>
    public partial interface IAccountTypeService
	{
        Task<IPagedList<AccountType>> GetAllIAccountTypeAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task InsertAccountTypeAsync(AccountType accountType);
        Task UpdateAccountTypeAsync(AccountType accountType);
        Task DeleteAccountTypeAsync(AccountType accountType);
        Task<IList<AccountType>> GetAccountTypeByIdsAsync(int[] accountTypeIds);
        Task<IPagedList<AccountType>> GetAllAccountTypeByNameAsync(string accountTypeName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<AccountType> GetOrCreateAccountTypeByNameAsync(string accountTypeName);
    }
}