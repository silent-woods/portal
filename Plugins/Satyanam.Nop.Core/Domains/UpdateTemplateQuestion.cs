using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateTemplateQuestion 
    /// </summary>
    public class UpdateTemplateQuestion : BaseEntity
    {

        public int UpdateTemplateId { get; set; }

        public string QuestionText { get; set; }

        public bool IsRequired { get; set; }

        public int ControlTypeId { get; set; }  // Backed by enum

        public int DisplayOrder { get; set; }

        public int? ValidationMinLength { get; set; }

        public int? ValidationMaxLength { get; set; }

        public int? ValidationFileMaximumSize { get; set; }

        public string ValidationFileAllowedExtensions { get; set; }

        public string DefaultValue { get; set; }

    }
}
