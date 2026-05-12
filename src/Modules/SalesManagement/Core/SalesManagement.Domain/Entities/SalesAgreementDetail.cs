namespace SalesManagement.Domain.Entities
{
    public class SalesAgreementDetail
    {
        public int Id { get; set; }

        public int SalesAgreementHeaderId { get; set; }
        public SalesAgreementHeader? SalesAgreementHeader { get; set; }

        // Cross-module FK → InventoryManagement.Item (resolved via IItemLookup). No DB FK constraint.
        public int ItemId { get; set; }

        // Cross-module FK → InventoryManagement.Item (variants are child Items with ParentId = ItemId).
        // Optional — null when the item has no variants.
        public int? VariantId { get; set; }

        public decimal AgreedRate { get; set; }
        public decimal TotalQty { get; set; }

        // System-maintained. Always 0 on create; updated when Sales Orders are linked to this agreement (future phase).
        public decimal ReleasedQty { get; set; }
    }
}
