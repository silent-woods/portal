using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateQuestionOption  
    /// </summary>
    public class UpdateQuestionOption : BaseEntity
    {
        public int UpdateTemplateQuestionId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPreSelected { get; set; }
        public bool IsRequired { get; set; }
    }
}
