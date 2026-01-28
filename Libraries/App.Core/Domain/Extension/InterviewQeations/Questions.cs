using App.Core;
using System;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a shipping by weight record
    /// </summary>
    public class Questions : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Recruitements identifier
        /// </summary>
        /// 
        public string Question { get; set; }
        public string Question_answers { get; set; }
        public int CategoryId { get; set; }
        public int QuestionTypeId { get; set; }

        public int DownloadId { get; set; }

        public int QuestionLevelId { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }


        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>

    }
}