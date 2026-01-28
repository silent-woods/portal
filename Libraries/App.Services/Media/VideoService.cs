using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Catalog;
using App.Core.Domain.Media;
using App.Data;
using App.Services.Catalog;

namespace App.Services.Media
{
    /// <summary>
    /// Video service
    /// </summary>
    public partial class VideoService : IVideoService
    {
        #region Fields

        private readonly IRepository<Video> _videoRepository;

        #endregion

        #region Ctor

        public VideoService(IRepository<Video> videoRepository)
        {
            _videoRepository = videoRepository;
        }

        #endregion

        #region CRUD methods

        /// <summary>
        /// Gets a video
        /// </summary>
        /// <param name="videoId">Video identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        public virtual async Task<Video> GetVideoByIdAsync(int videoId)
        {
            return await _videoRepository.GetByIdAsync(videoId, cache => default);
        }

        /// <summary>
        /// Inserts a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        public virtual async Task<Video> InsertVideoAsync(Video video)
        {
            await _videoRepository.InsertAsync(video);
            return video;
        }

        /// <summary>
        /// Updates the video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the video
        /// </returns>
        public virtual async Task<Video> UpdateVideoAsync(Video video)
        {
            await _videoRepository.UpdateAsync(video);
            
            return video;
        }

        /// <summary>
        /// Deletes a video
        /// </summary>
        /// <param name="video">Video</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteVideoAsync(Video video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            await _videoRepository.DeleteAsync(video);
        }

        #endregion
    }
}
