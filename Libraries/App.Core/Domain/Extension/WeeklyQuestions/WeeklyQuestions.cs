using App.Core;
using System;

namespace App.Core.Domain.WeeklyQuestion
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class WeeklyQuestions : BaseEntity
    {

        /// <summary>
        /// Gets or sets the WeeklyQuestions
        /// </summary>
        public string QuestionText { get; set; }
        public int ControlTypeId { get; set; }
        public string ControlValue { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public int DesignationId { get; set; }
    }
}