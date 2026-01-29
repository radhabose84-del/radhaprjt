using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster
{
    public class SpecificationMasterDTO : IMapFrom<SpecificationMasters>
    {
        public int Id { get; set; }
        public string? SpecificationName { get; set; }
        public int? AssetGroupId { get; set; }
        public byte ISDefault { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }   
        public string? GroupName { get; set; }
           
    }
}