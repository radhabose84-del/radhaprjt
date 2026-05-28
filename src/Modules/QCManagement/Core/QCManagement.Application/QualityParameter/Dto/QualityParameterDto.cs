namespace QCManagement.Application.QualityParameter.Dto
{
    public class QualityParameterDto
    {
        public int Id { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int ParameterGroupId { get; set; }
        public string? ParameterGroupCode { get; set; }
        public string? ParameterGroupName { get; set; }
        public int DataTypeId { get; set; }
        public string? DataTypeCode { get; set; }
        public string? DataTypeName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitCode { get; set; }
        public string? UnitName { get; set; }
        public int ValidationTypeId { get; set; }
        public string? ValidationTypeCode { get; set; }
        public string? ValidationTypeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
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
