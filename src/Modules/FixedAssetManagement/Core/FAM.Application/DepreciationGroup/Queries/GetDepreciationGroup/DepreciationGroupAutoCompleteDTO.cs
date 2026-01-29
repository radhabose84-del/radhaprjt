using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup
{
    public class DepreciationGroupAutoCompleteDTO  : IMapFrom<DepreciationGroups>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? DepreciationGroupName { get; set; } 
    }
}