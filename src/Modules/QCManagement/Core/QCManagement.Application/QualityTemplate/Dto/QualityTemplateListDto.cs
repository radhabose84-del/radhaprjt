namespace QCManagement.Application.QualityTemplate.Dto
{
    public class QualityTemplateListDto
    {
        public int Id { get; set; }
        public string? TemplateCode { get; set; }
        public string? TemplateName { get; set; }
        public string? Description { get; set; }
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
