using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item.PutAway
{
    public sealed class PutAwayStrategy : BaseEntity
    {
        public int PutAwayRuleId { get; set; }
        public PutAwayRule PutAwayRule { get; set; } = null!;
        public int StorageTypeId { get; set; }
        public MiscMaster MiscStorageTypeId { get; set; } = null!;
        public int? TargetId  { get; set; }        
        public int PriorityId { get; set; }
        public MiscMaster MiscPriority { get; set; } = null!;
    }
}