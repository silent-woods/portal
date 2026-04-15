using System;
using App.Core;

namespace Satyanam.Nop.Core.Domains;

public partial class LinkedInMessages : BaseEntity
{
    #region Properties

    public int LeadId { get; set; }

    public string Message { get; set; }

    public string ReceiverName { get; set; }

    public string ReceiverProfile { get; set; }

    public string ConversationId { get; set; }

    public string Direction { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    #endregion
}
