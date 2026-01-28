
using App.Web.Areas.Admin.Models.Extension.Announcements;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the announcement model factory interface
    /// </summary>
    public partial interface IAnnouncementModelFactory
    {
        /// <summary>
        /// Prepare announcement search model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the announcement search model
        /// </returns>
        Task<AnnouncementSearchModel> PrepareAnnouncementSearchModelAsync(AnnouncementSearchModel searchModel);

        /// <summary>
        /// Prepare paged announcement list model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the announcement list model
        /// </returns>
        Task<AnnouncementListModel> PrepareAnnouncementListModelAsync(AnnouncementSearchModel searchModel);

        /// <summary>
        /// Prepare announcement model
        /// </summary>
        /// <param name="model">Announcement model</param>
        /// <param name="announcement">Announcement entity</param>
        /// <param name="excludeProperties">Whether to exclude common properties</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the announcement model
        /// </returns>
        Task<AnnouncementModel> PrepareAnnouncementModelAsync(AnnouncementModel model, Announcement announcement, bool excludeProperties = false);
    }
}
