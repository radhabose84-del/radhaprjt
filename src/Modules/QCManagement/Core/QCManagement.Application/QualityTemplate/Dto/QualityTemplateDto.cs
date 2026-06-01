namespace QCManagement.Application.QualityTemplate.Dto
{
    public class QualityTemplateDto
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string? TemplateName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<QualityTemplateParameterDto>? Parameters { get; set; }

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

    public class QualityTemplateParameterDto
    {
        public int Id { get; set; }
        public int QualityParameterId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int SequenceNo { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsCritical { get; set; }
        public int? InspectionMethodId { get; set; }
        public string? InspectionMethodCode { get; set; }
        public string? InspectionMethodName { get; set; }
        public int? SampleSize { get; set; }
        public int? SampleUomId { get; set; }
        public string? SampleUomCode { get; set; }
        public string? SampleUomName { get; set; }
        public bool IsGradeApplicable { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public int? ParameterDataTypeId { get; set; }
        public string? ParameterDataTypeCode { get; set; }
        public string? ParameterDataTypeName { get; set; }
        public int? ParameterValidationTypeId { get; set; }
        public string? ParameterValidationTypeCode { get; set; }
        public string? ParameterValidationTypeName { get; set; }
        public int? ParameterUnitId { get; set; }
        public string? ParameterUnitCode { get; set; }
        public string? ParameterUnitName { get; set; }
    }
}
