using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule
{
    public sealed class PutAwayStrategyDto
    {
        public int Id { get; set; }
        public int StorageTypeId { get; set; }
        public string StorageTypeCode { get; set; } = "";
        public int? TargetId { get; set; }
        public string? TargetCode { get; set; }
        public string? TargetName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = "";     
        public Status IsActive { get; set; }    
    }
}