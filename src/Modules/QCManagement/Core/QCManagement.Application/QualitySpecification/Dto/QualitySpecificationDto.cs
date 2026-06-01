namespace QCManagement.Application.QualitySpecification.Dto
{
    public class QualitySpecificationDto
    {
        public int Id { get; set; }
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int QualityTemplateId { get; set; }
        public string? QualityTemplateCode { get; set; }
        public string? QualityTemplateName { get; set; }
        public int ApplicableLevelId { get; set; }
        public string? ApplicableLevelCode { get; set; }
        public string? ApplicableLevelName { get; set; }
        public int QcTypeId { get; set; }
        public string? QcTypeCode { get; set; }
        public string? QcTypeName { get; set; }
        public int? ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<QualitySpecificationParameterDto>? Parameters { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }

    public class QualitySpecificationParameterDto
    {
        public int Id { get; set; }
        public int QualityParameterId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public string? ParameterDataTypeCode { get; set; }
        public int ValidationTypeId { get; set; }
        public string? ValidationTypeCode { get; set; }
        public string? ValidationTypeName { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public List<string> AllowedValues { get; set; } = new();
        public int? SeverityId { get; set; }
        public string? SeverityCode { get; set; }
        public string? SeverityName { get; set; }
        public int? FailureActionId { get; set; }
        public string? FailureActionCode { get; set; }
        public string? FailureActionName { get; set; }
        public bool IsSamplingRequired { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
