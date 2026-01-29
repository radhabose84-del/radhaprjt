using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster
{
    public class SpecificationMasterAutoCompleteDTO : IMapFrom<SpecificationMasters>
    {
        public int Id { get; set; }        
        public string? SpecificationName { get; set; } 
        public byte ISDefault { get; set; }

    }
}