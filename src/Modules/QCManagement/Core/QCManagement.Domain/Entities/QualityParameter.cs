using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QualityParameter : BaseEntity
    {
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int ParameterGroupId { get; set; }
        public int DataTypeId { get; set; }
        public int? UnitId { get; set; }
        public int ValidationTypeId { get; set; }
        public string? Description { get; set; }

        public MiscMaster? ParameterGroup { get; set; }
        public MiscMaster? DataType { get; set; }
        public MiscMaster? ValidationType { get; set; }
    }
}
