namespace QCManagement.Application.QualitySpecification.Dto
{
    public class QualitySpecificationListDto
    {
        public int Id { get; set; }
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int QualityTemplateId { get; set; }
        public string? QualityTemplateName { get; set; }
        public int ApplicableLevelId { get; set; }
        public string? ApplicableLevelName { get; set; }
        public int? ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public string? AppliesTo { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public int ParameterCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

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
}
