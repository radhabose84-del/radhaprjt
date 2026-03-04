using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public MiscTypeMaster? MiscTypeMaster { get; set; }

        // Reverse navigation
        public ICollection<DispatchAddressMapping>? DispatchAddressMappings { get; set; }
        public ICollection<SalesContact>? SalesContacts { get; set; }

        // Reverse navigation (SalesOrder)
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsDiscountPlan { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsPaymentType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsFreightType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsCountList { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsEnquiryType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsDispatchLocationType { get; set; }
        public ICollection<SalesOrderDetail>? SalesOrderDetailsAsLineItemStatus { get; set; }

        // Reverse navigation (LotMaster)
        public ICollection<LotMaster>? LotMastersAsLotType { get; set; }
        public ICollection<LotMaster>? LotMastersAsStatus { get; set; }

        // Reverse navigation (Production)
        public ICollection<ProductionPackHeader>? ProductionPackHeadersAsStatus { get; set; }
        public ICollection<ProductionPackDetail>? ProductionPackDetailsAsQualityStatus { get; set; }

        // Reverse navigation (MovementTypeConfig)
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsMovementCategory { get; set; }
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsFromStockType { get; set; }
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsToStockType { get; set; }
    }
}
