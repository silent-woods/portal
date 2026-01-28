using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateTemplatePeriod  
    /// </summary>
    public class UpdateTemplatePeriod : BaseEntity
    {

        public int UpdateTemplateId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CreatedOnUtc { get; set; }

    }
}
