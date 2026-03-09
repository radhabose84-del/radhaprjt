
namespace SalesManagement.Domain.Common
{
    public static class MiscEnumEntity
    {       
        public const string CustomerVisitPath = "CustomerVisitPath";
        public const string CustomerVisit = "CustomerVisit";
        public const string LineItemApprovalStatus = "LineItemStatus";
        public const string LineStatusOpen  = "Open";
        public const string LineStatusPartiallyDispatched = "Partially Dispatched";
        public const string LineStatusClosed = "Closed";
        public const string SalesOrderVisitPath = "SalesOrderVisitPath";
        public const string AgentPoDocument = "AgentPoDocument";
        public const string QualityStatus = "QualityStatus";
        public const string Packed = "Packed";

        // Dispatch Advice Status
        public const string StockStatus = "StockStatus";
        public const string Reserved = "Reserved";
        public const string Invoiced = "Invoiced";
        public const string Dispatched = "Dispatched";
        public const string Pending = "Pending";

        // STO Line Item Status
        public const string StoLineItemStatus = "StoLineItemStatus";
        public const string StoLineStatusDraft = "Draft";

        // STO Header Status
        public const string StoHeaderStatus = "StoHeaderStatus";

        // Delivery Challan Status
        public const string DCLineStatus = "DCLineStatus";
        public const string DCStatusPending = "Pending";
        public const string DCStatusDispatched = "Dispatched";
        public const string DCStatusCancelled = "Cancelled";
        public const string DCStatusDelivered = "Delivered";

        // STO Receipt Status
        public const string StoReceiptLineStatus = "StoReceiptLineStatus";
        public const string StoReceiptStatusPending = "Pending";
        public const string StoReceiptStatusPartiallyReceived = "Partially Received";
        public const string StoReceiptStatusFullyReceived = "Fully Received";
        public const string StoReceiptStatusCancelled = "Cancelled";

    }
}
