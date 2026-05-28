using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public MiscTypeMaster? MiscTypeMaster { get; set; }

        public ICollection<QualityParameter>? QualityParametersAsParameterGroup { get; set; }
        public ICollection<QualityParameter>? QualityParametersAsDataType { get; set; }
        public ICollection<QualityParameter>? QualityParametersAsValidationType { get; set; }

        public ICollection<QualityTemplateParameter>? QualityTemplateParametersAsInspectionMethod { get; set; }

        // Reverse navigation for QualitySpecification (ApplicableLevel)
        public ICollection<QualitySpecification>? QualitySpecificationsAsApplicableLevel { get; set; }

        // Reverse navigation for QualitySpecificationParameter (ValidationType / Severity / FailureAction)
        public ICollection<QualitySpecificationParameter>? QualitySpecificationParametersAsValidationType { get; set; }
        public ICollection<QualitySpecificationParameter>? QualitySpecificationParametersAsSeverity { get; set; }
        public ICollection<QualitySpecificationParameter>? QualitySpecificationParametersAsFailureAction { get; set; }
    }
}
