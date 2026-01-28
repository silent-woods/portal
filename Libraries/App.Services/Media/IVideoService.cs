using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Media;

namespace App.Services.Media
{
    /// <summary>
    /// Video service interface
    /// </summary>
    public partial interface IVideoService
    {
        /// <summary>
        /// Gets a video
        /// </summary>
        /// <param name="videoId">Video identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        Task<Video> GetVideoByIdAsync(int videoId);

        /// <summary>
        /// Inserts a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        Task<Video> InsertVideoAsync(Video video);

        /// <summary>
        /// Updates the video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        Task<Video> UpdateVideoAsync(Video video);

        /// <summary>
        /// Deletes a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteVideoAsync(Video video);
    }
}