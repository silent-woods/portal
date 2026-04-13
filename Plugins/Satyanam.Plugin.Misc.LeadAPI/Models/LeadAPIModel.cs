using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.LeadAPI.Models;

public partial class LeadAPIModel
{
    #region Ctor

    public LeadAPIModel()
    {
        Tags = new List<int>();
    }

    #endregion

    #region Properties

    public string Name { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string? MobileNo { get; set; }
    public string Summary { get; set; }
    public string LinkedInUrl { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string ZipCode { get; set; }
    public string Industry { get; set; }
    public IList<int> Tags { get; set; }

    #endregion
}
