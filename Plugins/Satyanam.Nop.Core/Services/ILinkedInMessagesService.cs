using System.Threading.Tasks;
using Satyanam.Nop.Core.Domains;

namespace Satyanam.Nop.Core.Services;

public partial interface ILinkedInMessagesService
{
    #region Methdos

    Task InsertLinkedinMessageAsync(LinkedInMessages linkedInMessages);

    #endregion
}
