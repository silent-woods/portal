using System.ComponentModel;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  NoticePeriodDays Enum
    /// </summary>
    public enum NoticePeriodDaysEnum
    {
        Select = 0,
        Immediate = 1,
        [Description("15 Days")]
        FifteenDays = 2,

        [Description("30 Days")]
        ThirtyDays = 3,

        [Description("45 Days")]
        FortyFiveDays = 4,

        [Description("60 Days")]
        SixtyDays = 5,

        [Description("90 Days")]
        NinetyDays = 6
    }
}
