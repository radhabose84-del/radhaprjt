
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
        public const string LineStatusDeleted = "Deleted";
        public const string SalesOrderVisitPath = "SalesOrderVisitPath";
        public const string AgentPoDocument = "AgentPoDocument";
        public const string SalesOrderMdApprovalPath = "SalesOrderMdApprovalPath";
        public const string QualityStatus = "QualityStatus";
        public const string Packed = "Packed";

        // Stock Status (used by Dispatch Advice + Sales Return; consolidated 2026-05-07 — formerly BagStatus)
        public const string StockStatus = "StockStatus";
        public const string Reserved = "Reserved";
        public const string Invoiced = "Invoiced";
        public const string Dispatched = "Dispatched";
        public const string Damaged = "Damaged";
        public const string Pending = "Pending";
        public const string StockStatusDefect = "DEFECT";
        public const string StockStatusDamaged = "DAMAGED";
        public const string StockStatusYarnMismatch = "YARN MISMATCH";

        // Invoice Approval Status
        public const string InvoiceApprovalStatus = "ApprovalStatus";
        public const string InvoiceStatusPending = "Pending";
        public const string InvoiceStatusApproved = "Approved";

        // SalesOrder Approval Status
        public const string SalesOrderApprovalStatus = "ApprovalStatus";
        public const string SalesOrderStatusPending = "Pending";
        public const string SalesOrderStatusApproved = "Approved";
        public const string SalesOrderStatusRejected = "Rejected";
        public const string SalesOrderStatusCancelled = "Cancelled";
        public const string SalesOrderStatusForeClosed = "ForeClosed";

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
        public const string TransactionTypeSalesOrderAmendment = "Sales Order Amendment";

        // Document Sequence - Transaction Types
        public const string TransactionTypeSalesLead = "Sales Lead";
        public const string TransactionTypeSalesEnquiry = "Sales Enquiry";
        public const string TransactionTypeSalesQuotation = "Sales Quotation";
        public const string TransactionTypeSalesQuotationAmendment = "Sales Quotation Amendment";
        public const string TransactionTypeDispatchAdvice = "Dispatch Advice";
        public const string TransactionTypeInvoice = "Invoice";
        public const string TransactionTypePriceMaster = "PriceMaster";
        public const string TransactionTypePackMaster = "PackMaster";
        public const string TransactionTypeSto = "Stock Transfer Order";
        public const string TransactionTypeStodc = "STO Delivery Challan";
        public const string TransactionTypeStogr = "STO Goods Receipt";
        public const string TransactionTypeTripSheet = "Trip Sheet";
        public const string ModuleSales = "Sales";

        // Complaint Workflow
        public const string ComplaintApprovalStatus = "ApprovalStatus";
        public const string ComplaintApprovalPending = "Pending";
        public const string ComplaintApprovalApproved = "Approved";
        public const string ComplaintApprovalRejected = "Rejected";
        public const string ComplaintModuleTypeName = "Complaints";
        public const string ComplaintQCReviewModuleTypeName = "QC Review";
        public const string ComplaintResolutionModuleTypeName = "Resolution";
        public const string TransactionTypeComplaint = "Complaint";

        // Complaint Notifications (NotificationConfig.ModuleName values; configs Ids 31-36)
        public const string NotifModuleNewComplaint        = "New Complaint";
        public const string NotifModuleComplaintMoApproval = "Complaint MO Approval";
        public const string NotifModuleQcReviewSubmitted   = "QC Review Submitted";
        public const string NotifModuleQcReviewApproved    = "QC Review Approved";
        public const string NotifModuleFeedbackRequested   = "Feedback Requested";
        public const string NotifModuleResolutionSubmitted = "Resolution Submitted";

        // Rejection notifications (Phase 3 — symmetric "Rejected" counterparts to the
        // Approval module names above). Each fires when the workflow approver clicks
        // Reject. Bell goes to the SUBMITTER of the rejected stage (complaint creator
        // for Complaint, ReviewedBy for QC, ResolvedBy for Resolution).
        public const string NotifModuleComplaintRejected   = "Complaint Rejected";
        public const string NotifModuleQcReviewRejected    = "QC Review Rejected";
        public const string NotifModuleResolutionRejected  = "Resolution Rejected";

        // Notification EventType lookup keys (resolved at runtime via IAppDataMiscMasterLookup
        // → AppData.MiscMaster). Codes are stable across environments; Ids are auto-generated and
        // must NEVER be hardcoded.
        public const string NotifEventTypeMiscType = "EventType";
        public const string NotifEventTypeCreate   = "Create";

        // ApprovalStepDetail.TargetType code for the Complaint QC-Review step.
        // Used to dynamically resolve QC-reviewer InApp recipients in C#
        // (mirrors AppData.sp_EvaluateApproval Block 4) instead of the static
        // WorkFlow_GetUserId / NotificationLevelHierarchy seed.
        public const string ComplaintQcReviewerTargetType = "COMPLAINT_QC_REVIEWER_USER";

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

        // Resolution Type
        public const string ResolutionType = "ResolutionType";
        public const string ResolutionSalesReturn = "Sales Return";
        public const string ResolutionCreditNote = "Credit Note";
        public const string ResolutionReplacement = "Replacement";
        public const string ResolutionReprocess = "Reprocess";
        public const string ResolutionNoAction = "No Action";

        // Return Status
        public const string ReturnStatus = "ReturnStatus";
        public const string ReturnStatusPending = "Pending";
        public const string ReturnStatusReceived = "Received";
        public const string ReturnStatusPartiallyReturned = "PartiallyReturned";
        public const string ReturnStatusFullyReturned = "FullyReturned";

        // Sales Return
        public const string TransactionTypeSalesReturn = "Sales Return";

        // Stock Entry Type
        public const string StockEntryType = "StockEntryType";
        public const string StockEntryTypeSalesReturn = "SalesReturn";

        // Closure Status
        public const string ClosureStatus = "ClosureStatus";
        public const string ClosureStatusOpen = "Open";
        public const string ClosureStatusReadyForClosure = "Ready for Closure";
        public const string ClosureStatusClosed = "Closed";

        // STO Receipt Status
        public const string StoReceiptLineStatus = "StoReceiptLineStatus";
        public const string StoReceiptStatusPending = "Pending";
        public const string StoReceiptStatusPartiallyReceived = "Partially Received";
        public const string StoReceiptStatusFullyReceived = "Fully Received";
        public const string StoReceiptStatusCancelled = "Cancelled";

        // Dispatch Advice Address Based Freight
        public const string DispatchAddressType = "DispatchAddressType";
        public const string DirectToParty = "Direct-To-Party";
        public const string Others = "Others";

        // Discount Slab Type
        public const string SLAB_TYPE = "SLAB_TYPE";
        public const string QUANTITY = "QUANTITY";
        public const string AMOUNT = "AMOUNT";
        public const string PAYMENT_DAYS = "PAYMENT_DAYS";

        // Proforma Invoice Status
        public const string ProformaInvoiceStatus = "ProformaInvoiceStatus";
        public const string ProformaStatusDraft = "Draft";
        public const string ProformaStatusSent = "Sent";
        public const string ProformaStatusCancelled = "Cancelled";
        public const string ProformaStatusPaid = "Paid";
        public const string ProformaStatusPartiallyPaid = "Partially Paid";

        // Document Sequence - Proforma Invoice
        public const string TransactionTypeProformaInvoice = "Proforma Invoice";

        // Payment Type (for advance payment check)
        public const string PaymentTypeMiscType = "PaymentType";
        public const string PaymentTypeAdvance = "Advance";

        // Sales Order Type Master — variants that skip workflow + notifications and persist with
        // StatusId pre-set to "Approved".
        public const string SalesOrderTypeRateAgreement = "Rate Agreement";
        public const string SalesOrderTypeSampleOrder   = "Sample Order";

        // Sales Agreement
        public const string TransactionTypeSalesAgreement = "Sales Agreement";
        public const string SalesAgreementApprovalStatus = "ApprovalStatus";
        public const string SalesAgreementStatusApproved = "Approved";
        public const string SalesAgreementStatusCancelled = "Cancelled";

    }
}
    