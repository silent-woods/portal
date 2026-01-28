using System;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AzureIntegration.Models.AzureResponse;

public partial class WorkItem
{
    #region Properties

    public int Id { get; set; }

    public int Rev { get; set; }

    public Fields Fields { get; set; }

    public Links _Links { get; set; }

    public string Url { get; set; }

    #endregion
}

public partial class Fields
{
    #region Properties

    public string SystemAreaPath { get; set; }

    public string SystemTeamProject { get; set; }

    public string SystemIterationPath { get; set; }

    public string SystemWorkItemType { get; set; }

    public string SystemState { get; set; }

    public string SystemReason { get; set; }

    public DateTime SystemCreatedDate { get; set; }

    public IdentityRef SystemCreatedBy { get; set; }

    public DateTime SystemChangedDate { get; set; }

    public IdentityRef SystemChangedBy { get; set; }

    public string SystemTitle { get; set; }

    public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }

    public int MicrosoftVSTSCommonPriority { get; set; }

    #endregion
}

public partial class IdentityRef
{
    #region Properties

    public string DisplayName { get; set; }

    public string Url { get; set; }

    public Dictionary<string, AvatarLink> _Links { get; set; }

    public string Id { get; set; }

    public string UniqueName { get; set; }

    public string ImageUrl { get; set; }

    public string Descriptor { get; set; }

    #endregion
}

public class AvatarLink
{
    #region Properties

    public string Href { get; set; }

    #endregion
}

public partial class Links
{
    #region Properties

    public Link Self { get; set; }

    public Link WorkItemUpdates { get; set; }

    public Link WorkItemRevisions { get; set; }

    public Link WorkItemHistory { get; set; }

    public Link Html { get; set; }

    public Link WorkItemType { get; set; }

    public Link Fields { get; set; }

    #endregion
}

public partial class Link
{
    #region Properties

    public string Href { get; set; }

    #endregion
}
