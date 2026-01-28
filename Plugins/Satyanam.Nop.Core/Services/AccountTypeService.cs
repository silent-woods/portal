using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
	/// <summary>
	/// AccountType service
	/// </summary>
	public partial class AccountTypeService : IAccountTypeService
	{
		#region Fields

		private readonly IRepository<AccountType> _accountTypeRepository;

		#endregion

		#region Ctor

		public AccountTypeService(IRepository<AccountType> accountTypeRepository)
		{
			_accountTypeRepository = accountTypeRepository;
		}

		#endregion

		#region Methods

		#region AccountType

		/// <summary>
		/// Gets all AccountType
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="accountType">Filter by accountType name</param>
		/// <returns>
		/// A task that represents the asynchronous operation
		/// The task result contains the accountType
		/// </returns>

		public virtual async Task<IPagedList<AccountType>> GetAllIAccountTypeAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
		{
			var query = await _accountTypeRepository.GetAllAsync(async query =>
			{
				if (!string.IsNullOrWhiteSpace(name))
					query = query.Where(c => c.Name.Contains(name));

				return query.OrderBy(c => c.Id);
			});
			//paging
			return new PagedList<AccountType>(query.ToList(), pageIndex, pageSize);
		}

		/// <summary>
		/// Gets a AccountType
		/// </summary>
		/// <param name="accountTypeId">accountType identifier</param>
		/// <returns>
		/// A task that represents the asynchronous operation
		/// The task result contains the accountType
		/// </returns>
		public virtual async Task<AccountType> GetAccountTypeByIdAsync(int accountTypeId)
		{
			return await _accountTypeRepository.GetByIdAsync(accountTypeId);
		}

		public virtual async Task<IList<AccountType>> GetAccountTypeByIdsAsync(int[] accountTypeIds)
		{
			return await _accountTypeRepository.GetByIdsAsync(accountTypeIds);
		}


		/// <summary>
		/// Inserts a AccountType
		/// </summary>
		/// <param name="accountType">AccountType</param>
		/// <returns>A task that represents the asynchronous operation</returns>
		public virtual async Task InsertAccountTypeAsync(AccountType accountType)
		{
			await _accountTypeRepository.InsertAsync(accountType);
		}

		/// <summary>
		/// Updates the AccountType
		/// </summary>
		/// <param name="accountType">accountType</param>
		/// <returns>A task that represents the asynchronous operation</returns>
		public virtual async Task UpdateAccountTypeAsync(AccountType accountType)
		{
			await _accountTypeRepository.UpdateAsync(accountType);
		}

		/// <summary>
		/// Deletes a AccountType
		/// </summary>
		/// <param name="accountType">AccountType</param>
		/// <returns>A task that represents the asynchronous operation</returns>
		public virtual async Task DeleteAccountTypeAsync(AccountType accountType)
		{
			await _accountTypeRepository.DeleteAsync(accountType);
		}

		public virtual async Task<IPagedList<AccountType>> GetAllAccountTypeByNameAsync(string accountTypeName,
			int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
		{
			var query = await _accountTypeRepository.GetAllAsync(async query =>
			{
				return query.OrderByDescending(c => c.Id);
			});
			//paging
			return new PagedList<AccountType>(query.ToList(), pageIndex, pageSize);
		}
		private static readonly Dictionary<string, string> AccountTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
	{ "Analyst", "Anamlyst" },
	{ "analyst", "Anamlyst" },
	{ "Competitor", "Competitor" },
	{ "competitor", "Competitor" },
	{ "Customer", "Customer" },
	{ "customer", "Customer" },
	{ "Distributor", "Distributor" },
	{ "distributor", "Distributor" },
	{ "Integartor", "Integartor" },
	{ "integartor", "Integartor" },
	{ "Investor", "Investor" },
	{ "investor", "Investor" },
	{ "Other", "Other" },
	{ "other", "Other" },
	{ "Partner", "Partner" },
	{ "partner", "Partner" },
	{ "Press", "Press" },
	{ "press", "Press" },
	{ "Prospect", "Prospect" },
	{ "prospect", "Prospect" },
	{ "Reseller", "Reseller" },
	{ "reseller", "Reseller" },
	{ "Supplier", "Supplier" },
	{ "supplier", "Supplier" },
	{ "Vendor", "Vendor" },
	{ "vendor", "Vendor" },

};
		private string NormalizeAccountType(string accountTypeName)
		{
			if (string.IsNullOrWhiteSpace(accountTypeName))
				return null;

			// Check if the accountType exists in the mapping
			return AccountTypeMappings.TryGetValue(accountTypeName.Trim(), out var normalizedAccountType) ? normalizedAccountType : accountTypeName.Trim();
		}
		public async Task<AccountType> GetOrCreateAccountTypeByNameAsync(string accountTypeName)
		{
			if (string.IsNullOrWhiteSpace(accountTypeName))
				return null;

			var normalizedAccountTypeName = NormalizeAccountType(accountTypeName);

			// Try to find an existing accountType
			var existingAccountType = (await GetAllIAccountTypeAsync(normalizedAccountTypeName)).FirstOrDefault();
			if (existingAccountType != null)
				return existingAccountType;

			// Create new accountType if not found
			var newAccountType = new AccountType { Name = normalizedAccountTypeName };
			await _accountTypeRepository.InsertAsync(newAccountType);
			return newAccountType;
		}
		#endregion

		#endregion
	}
}