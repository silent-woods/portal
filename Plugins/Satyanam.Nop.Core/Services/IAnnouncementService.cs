using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Announcement service interface
    /// </summary>
    public partial interface IAnnouncementService
    {
        /// <summary>
        /// Gets all announcements
        /// </summary>
        Task<IPagedList<Announcement>> GetAllAnnouncementsAsync(
            string title = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false);

        /// <summary>
        /// Gets an announcement by Id
        /// </summary>
        Task<Announcement> GetAnnouncementByIdAsync(int id);

        /// <summary>
        /// Gets announcements by multiple ids
        /// </summary>
        Task<IList<Announcement>> GetAnnouncementsByIdsAsync(int[] ids);

        /// <summary>
        /// Inserts an announcement
        /// </summary>
        Task InsertAnnouncementAsync(Announcement announcement);

        /// <summary>
        /// Updates an announcement
        /// </summary>
        Task UpdateAnnouncementAsync(Announcement announcement);

        /// <summary>
        /// Deletes an announcement
        /// </summary>
        Task DeleteAnnouncementAsync(Announcement announcement);

       

        /// <summary>
        /// Get all announcements targeted to a specific employee
        /// </summary>
        Task<IList<Announcement>> GetAnnouncementsForEmployeeAsync(int employeeId);
    }
}
