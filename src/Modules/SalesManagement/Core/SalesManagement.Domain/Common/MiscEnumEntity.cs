
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
        public const string InvoiceStatusApproved = "Approved";

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
        public const string ComplaintModuleTypeName = "Complaints";
        public const string TransactionTypeComplaint = "Complaint";

        // QC Review
        public const string PhysicalVerificationStatus = "PhysicalVerification";
        public const string PhysicalVerificationPending = "Pending";
        public const string PhysicalVerificationInProgress = "In Progress";
        public const string PhysicalVerificationCompleted = "Completed";

        public const string QCComplaintStatus = "QCComplaintStatus";
        public const string QCAccepted = "QC Accepted";
        public const string QCRejected = "QC Rejected";

        public const string ComplaintSeverity = "ComplaintSeverity";
        public const string SeverityMinor = "Minor";
        public const string SeverityMajor = "Major";
        public const string SeverityCritical = "Critical";

        public const string CompensationStructure = "CompStructure";
        public const string CompensationCreditNote = "Credit Note";
        public const string CompensationReplacement = "Replacement";
        public const string CompensationReprocess = "Reprocess";
        public const string CompensationNone = "No Compensation";

        public const string QCAssignmentRole = "QCAssignmentRole";
        public const string RoleProductionFM = "Production/FM";
        public const string RoleMaintenance = "Maintenance";
        public const string RoleQCLab = "QC Lab";
        public const string RoleOthers = "Others";

        public const string QCAssignmentStatus = "QCAssignmentStatus";
        public const string AssignmentPending = "Pending";
        public const string AssignmentSubmitted = "Submitted";

        // Department Feedback Status
        public const string FeedbackStatus = "FeedbackStatus";
        public const string FeedbackPending = "Pending";
        public const string FeedbackSubmitted = "Submitted";
        public const string FeedbackReworkRequired = "Rework Required";

        // Root Cause Category
        public const string RootCauseCategory = "RootCauseCategory";

        // STO Receipt Status
        public const string StoReceiptLineStatus = "StoReceiptLineStatus";
        public const string StoReceiptStatusPending = "Pending";
        public const string StoReceiptStatusPartiallyReceived = "Partially Received";
        public const string StoReceiptStatusFullyReceived = "Fully Received";
        public const string StoReceiptStatusCancelled = "Cancelled";

    }
}
