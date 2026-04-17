using System;
using System.Threading.Tasks;
using App.Data;
using Satyanam.Nop.Core.Domains;

namespace Satyanam.Nop.Core.Services;

public partial class LinkedInMessagesService : ILinkedInMessagesService
{
    #region Fields

    protected readonly IRepository<LinkedInMessages> _linkedinMessagesRepository;

    #endregion

    #region Ctor

    public LinkedInMessagesService(IRepository<LinkedInMessages> linkedinMessagesRepository)
    {
        _linkedinMessagesRepository = linkedinMessagesRepository;
    }

    #endregion

    #region Methods

    public virtual async Task InsertLinkedinMessageAsync(LinkedInMessages linkedInMessages)
    {
        ArgumentNullException.ThrowIfNull(nameof(linkedInMessages));

        await _linkedinMessagesRepository.InsertAsync(linkedInMessages);
    }

    #endregion
}
