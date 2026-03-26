
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
        public const string Damaged = "Damaged";
        public const string Pending = "Pending";

        // Invoice Approval Status
        public const string InvoiceApprovalStatus = "ApprovalStatus";
        public const string InvoiceStatusPending = "Pending";

        // SalesOrder Approval Status
        public const string SalesOrderApprovalStatus = "ApprovalStatus";
        public const string SalesOrderStatusPending = "Pending";
        public const string SalesOrderStatusApproved = "Approved";
        public const string SalesOrderStatusRejected = "Rejected";

        // STO Line Item Status
        public const string StoLineItemStatus = "StoLineItemStatus";
        public const string StoLineStatusDraft = "Draft";

        // STO Header Status
        public const string StoHeaderStatus = "StoHeaderStatus";
        public const string StoHeaderStatusPending = "Pending";

        // STO Approval (workflow)
        public const string StoApprovalStatus = "ApprovalStatus";
        public const string StoApprovalPending = "Pending";
        public const string StoApprovalApproved = "Approved";
        public const string StoApprovalRejected = "Rejected";
        public const string StoModuleTypeName = "STO";

        // Delivery Challan Status
        public const string DCLineStatus = "DCLineStatus";
        public const string DCStatusPending = "Pending";
        public const string DCStatusDispatched = "Dispatched";
        public const string DCStatusCancelled = "Cancelled";
        public const string DCStatusDelivered = "Delivered";
        public const string DCModuleTypeName = "Delivery Challan";

        // Workflow Module Type Names
        public const string TransactionTypeSalesOrder = "Sales Order";

        // Document Sequence - Transaction Types
        public const string TransactionTypeDispatchAdvice = "Dispatch Advice";
        public const string TransactionTypeInvoice = "Invoice";
        public const string TransactionTypePriceMaster = "PriceMaster";
        public const string TransactionTypePackMaster = "PackMaster";
        public const string TransactionTypeSto = "Stock Transfer Order";
        public const string TransactionTypeStodc = "STO Delivery Challan";
        public const string TransactionTypeStogr = "STO Goods Receipt";
        public const string ModuleSales = "Sales";

        // Complaint Workflow
        public const string ComplaintApprovalStatus = "ApprovalStatus";
        public const string ComplaintApprovalPending = "Pending";
        public const string ComplaintModuleTypeName = "Complaint";
        public const string TransactionTypeComplaint = "Complaint";

        // STO Receipt Status
        public const string StoReceiptLineStatus = "StoReceiptLineStatus";
        public const string StoReceiptStatusPending = "Pending";
        public const string StoReceiptStatusPartiallyReceived = "Partially Received";
        public const string StoReceiptStatusFullyReceived = "Fully Received";
        public const string StoReceiptStatusCancelled = "Cancelled";

    }
}
