
using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup
{
    public class DepreciationGroupDTO  : IMapFrom<DepreciationGroups>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? BookType { get; set; }
        public string? DepreciationGroupName { get; set; }        
        public int? AssetGroupId { get; set; } 
        public string? AssetGroupName { get; set; } 
        public int? UsefulLife { get; set; }
        public string? DepreciationMethod { get; set; }
        public int? ResidualValue { get; set; }
        public int SortOrder { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset?  CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset?  ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public string? DepreciationMethodDesc { get; set; }
        public string? BookTypeDesc { get; set; }  
        public IsDelete IsDeleted { get; set; }      
    }
}
