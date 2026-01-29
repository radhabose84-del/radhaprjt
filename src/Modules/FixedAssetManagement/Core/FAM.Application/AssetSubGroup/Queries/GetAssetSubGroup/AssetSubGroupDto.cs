using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup
{
    public class AssetSubGroupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? SubGroupName { get; set; }
        public int SortOrder { get; set; }
        public int GroupId { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public decimal SubGroupPercentage { get; set; }       
        public byte AdditionalDepreciation { get; set; }    
    }
}