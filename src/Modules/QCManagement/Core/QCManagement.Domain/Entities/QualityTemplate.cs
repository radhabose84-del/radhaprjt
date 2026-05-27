using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QualityTemplate : BaseEntity
    {
        public string? TemplateCode { get; set; }
        public string? TemplateName { get; set; }
        public string? Description { get; set; }

        // Child collection
        public ICollection<QualityTemplateParameter>? QualityTemplateParameters { get; set; }
    }
}
