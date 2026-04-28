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
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsPaymentType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsFreightType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsCountList { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsEnquiryType { get; set; }
        public ICollection<SalesOrderHeader>? SalesOrderHeadersAsStatus { get; set; }
        public ICollection<SalesOrderDetail>? SalesOrderDetailsAsLineItemStatus { get; set; }

        // Reverse navigation (MovementTypeConfig)
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsMovementCategory { get; set; }
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsFromStockType { get; set; }
        public ICollection<MovementTypeConfig>? MovementTypeConfigsAsToStockType { get; set; }

        // Reverse navigation (DispatchAdvice)
        public ICollection<DispatchAdviceHeader>? DispatchAdviceHeadersAsStatus { get; set; }
        public ICollection<DispatchAdviceHeader>? DispatchAdviceHeadersAsDispatchType { get; set; }

        // Reverse navigation (SalesOrderDiscount)
        public ICollection<SalesOrderDiscount>? SalesOrderDiscountsAsSlabType { get; set; }

        // Reverse navigation (StoDetail)
        public ICollection<StoDetail>? StoDetailsAsLineStatus { get; set; }

        // Reverse navigation (StoHeader)
        public ICollection<StoHeader>? StoHeadersAsHeaderStatus { get; set; }

        // Reverse navigation (DeliveryChallanHeader)
        public ICollection<DeliveryChallanHeader>? DeliveryChallanHeadersAsStatus { get; set; }
        public ICollection<DeliveryChallanHeader>? DeliveryChallanHeadersAsDcType { get; set; }

        // Reverse navigation (InvoiceHeader)
        public ICollection<InvoiceHeader>? InvoiceHeadersAsTransportMode { get; set; }
        public ICollection<InvoiceHeader>? InvoiceHeadersAsStatus { get; set; }

        // Reverse navigation (ItemPriceMaster)
        public ICollection<ItemPriceMaster>? ItemPriceMastersAsStatus { get; set; }

        // Reverse navigation (SalesQuotationHeader)
        public ICollection<SalesQuotationHeader>? SalesQuotationHeadersAsStatus { get; set; }

        // Reverse navigation (StoReceipt)
        public ICollection<StoReceiptHeader>? StoReceiptHeadersAsStatus { get; set; }
        public ICollection<StoReceiptDetail>? StoReceiptDetailsAsLineStatus { get; set; }

        // Reverse navigation (AgentCommissionConfig)
        public ICollection<AgentCommissionConfig>? AgentCommissionConfigsAsCommissionBasis { get; set; }
        public ICollection<AgentCommissionConfig>? AgentCommissionConfigsAsApplicableLevel { get; set; }

        // Reverse navigation (SalesOrderAmendment)
        public ICollection<SalesOrderAmendmentHeader>? SalesOrderAmendmentHeadersAsStatus { get; set; }

        // Reverse navigation (DiscountMaster)
        public ICollection<DiscountMaster>? DiscountMastersAsTriggerEvent { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsDiscountBasis { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsExecutionType { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsCustomerGroup { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsMaxDiscountLimitType { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsExclusionGroup { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsValueType { get; set; }
        public ICollection<DiscountMaster>? DiscountMastersAsSlabType { get; set; }

        // Reverse navigation (CommissionSplitDetail)
        public ICollection<CommissionSplitDetail>? CommissionSplitDetailsAsRole { get; set; }
        public ICollection<CommissionSplitDetail>? CommissionSplitDetailsAsShareType { get; set; }

        // Reverse navigation (AgentCommissionConfig — TriggerEvent, SlabType)
        public ICollection<AgentCommissionConfig>? AgentCommissionConfigsAsTriggerEvent { get; set; }
        public ICollection<AgentCommissionConfig>? AgentCommissionConfigsAsSlabType { get; set; }

        // Reverse navigation (AgentCommissionSlab — slab-level CommissionType, CommissionBasis)
        public ICollection<AgentCommissionSlab>? AgentCommissionSlabsAsCommissionType { get; set; }
        public ICollection<AgentCommissionSlab>? AgentCommissionSlabsAsCommissionBasis { get; set; }

        // Reverse navigation (ProformaInvoice)
        public ICollection<ProformaInvoice>? ProformaInvoicesAsStatus { get; set; }

        // Reverse navigation (SalesOrderTypeMaster — SoType FK)
        public ICollection<SalesOrderTypeMaster>? SalesOrderTypeMastersAsSoType { get; set; }

    }
}
