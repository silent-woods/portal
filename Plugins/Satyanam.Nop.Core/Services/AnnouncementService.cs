using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;  // Your Announcement entity
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Announcement service
    /// </summary>
    public partial class AnnouncementService : IAnnouncementService
    {
        #region Fields

        private readonly IRepository<Announcement> _announcementRepository;

        #endregion

        #region Ctor

        public AnnouncementService(IRepository<Announcement> announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all announcements
        /// </summary>
        public virtual async Task<IPagedList<Announcement>> GetAllAnnouncementsAsync(
            string title = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _announcementRepository.Table;

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(a => a.Title.Contains(title));

           

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return await Task.FromResult(new PagedList<Announcement>(query.ToList(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets an announcement by Id
        /// </summary>
        public virtual async Task<Announcement> GetAnnouncementByIdAsync(int id)
        {
            return await _announcementRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets announcements by multiple ids
        /// </summary>
        public virtual async Task<IList<Announcement>> GetAnnouncementsByIdsAsync(int[] ids)
        {
            return await _announcementRepository.GetByIdsAsync(ids);
        }

        /// <summary>
        /// Inserts an announcement
        /// </summary>
        public virtual async Task InsertAnnouncementAsync(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            announcement.CreatedOnUtc = DateTime.UtcNow;
            await _announcementRepository.InsertAsync(announcement);
        }

        /// <summary>
        /// Updates an announcement
        /// </summary>
        public virtual async Task UpdateAnnouncementAsync(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            await _announcementRepository.UpdateAsync(announcement);
        }

        /// <summary>
        /// Deletes an announcement
        /// </summary>
        public virtual async Task DeleteAnnouncementAsync(Announcement announcement)
        {
            if (announcement == null)
                throw new ArgumentNullException(nameof(announcement));

            await _announcementRepository.DeleteAsync(announcement);
        }


        /// <summary>
        /// Get all announcements targeted to a specific employee
        /// (based on SendEmployeeIds or AudienceType & ReferenceIds)
        /// </summary>
        public virtual async Task<IList<Announcement>> GetAnnouncementsForEmployeeAsync(int employeeId)
        {
 
           var query = _announcementRepository.Table.Where(a =>
                (a.SendEmployeeIds != null && a.SendEmployeeIds.Contains(employeeId.ToString())) ||
                (a.LikedEmployeeIds != null && a.LikedEmployeeIds.Contains(employeeId.ToString())) // optional
            );

            return await Task.FromResult(query.ToList());
        }

        #endregion
    }
}
