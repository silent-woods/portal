using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.EmailVerification.Services
{
    /// <summary>
    /// Represents service shipping by weight service
    /// </summary>
    public interface IEmailverificationService
    {
        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>
        Task<string> VerifyEmailApi(string replyToEmailAddress);


    }
}
