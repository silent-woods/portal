using System;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Affiliates;

namespace App.Services.Affiliates
{
    /// <summary>
    /// Affiliate service interface
    /// </summary>
    public partial interface IAffiliateService
    {
        /// <summary>
        /// Gets an affiliate by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate
        /// </returns>
        Task<Affiliate> GetAffiliateByIdAsync(int affiliateId);

        /// <summary>
        /// Gets an affiliate by friendly URL name
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate
        /// </returns>
        Task<Affiliate> GetAffiliateByFriendlyUrlNameAsync(string friendlyUrlName);

        /// <summary>
        /// Marks affiliate as deleted 
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAffiliateAsync(Affiliate affiliate);

        /// <summary>
        /// Gets all affiliates
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name; null to load all records</param>
        /// <param name="firstName">First name; null to load all records</param>
        /// <param name="lastName">Last name; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliates
        /// </returns>
        Task<IPagedList<Affiliate>> GetAllAffiliatesAsync(string friendlyUrlName = null,
            string firstName = null, string lastName = null,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false);

        /// <summary>
        /// Inserts an affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAffiliateAsync(Affiliate affiliate);

        /// <summary>
        /// Updates the affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAffiliateAsync(Affiliate affiliate);

        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate full name
        /// </returns>
        Task<string> GetAffiliateFullNameAsync(Affiliate affiliate);

        /// <summary>
        /// Generate affiliate URL
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated affiliate URL
        /// </returns>
        Task<string> GenerateUrlAsync(Affiliate affiliate);

        /// <summary>
        /// Validate friendly URL name
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <param name="friendlyUrlName">Friendly URL name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the valid friendly name
        /// </returns>
        Task<string> ValidateFriendlyUrlNameAsync(Affiliate affiliate, string friendlyUrlName);
    }
}