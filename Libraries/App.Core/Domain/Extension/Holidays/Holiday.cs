using System;

namespace App.Core.Domain.Holidays
{
    /// <summary>
    /// Holiday
    /// </summary>
    public partial class Holiday : BaseEntity
    {
        /// <summary>
        /// Gets or sets the holiday festival
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the holiday date 
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the holiday week day
        /// </summary>
        public string WeekDay { get; set; }
    }
}
