namespace PurchaseManagement.Domain.Common
{
    public class MiscEnumEntity
    {
        public const string Status = "Status";
        public const string Open = "Open";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Deleted = "Deleted";
        public const string PurchaseIndent = "Purchase Indent";
        public const string Pending = "Pending";
        public const string Draft = "Draft";
        public const string Submit = "Submit";
        public const string ImagePath = "ImagePath";
        public const string QuotationImage = "QuotationImage";
        public const string RFQ = "RFQ";
        public const string ApprovalStatus = "ApprovalStatus";
        public const string SourceFrom = "SourceFrom";
        public const string QuotationComparison = "Quotation Comparison";
        public const string Quotation = "Quotation";
        public const string SourceFromDirect = "Direct";
        public const string PriceMaster = "Item Price Master";
        public const string GateEntryImage = "GateEntryImage";
        public const string POImage = "POImage";
        public const string POLocal = "Purchase Order";
        public const string POImport = "POImport";
        public const string GrnReceivedImage = "GrnReceivedImage";
        public const string QcGrnImage = "QcGrnImage";
        public const string Consumption = "Consumption";
        public const string SubStores = "SubStores";
        public const string serviceCategory = "OneTime";
        public const string MaterialRequest = "Material Request";
        public const string ServicePO = "ServicePO";
        public const string ServiceOrderType = "ServiceOrderType";
        public const string POMethod = "POMethod";
        public const string Local = "Local";
        public const string Import = "Import";
        public const string Contract = "Contract";
        public const string POCategoryService = "Service";
        public const string Capital = "Capital";
        public const string ServiceCategoryRecurring = "Recurring";
        public const string IssueReturn = "Issue Return";
        public const string DocumentPath = "PoDocument";
        public const string DocumentImagePath = "PODocumentImagePath";

        public const string ServiceEntrySheet = "Service Entry Sheet";
        public const string Incoterms = "Incoterms";
        public const string EmergencyPO = "Emergency";

        public const string Cancelled = "Cancelled";
        public const string ForeClosed = "ForeClosed";
        public const string TransactionTypeLPO = "Local Purchase Order";
        public const string TransactionTypeCPO = "Contract Purchase Order";
        public const string TransactionTypeIPO = "Import Purchase Order";
		public const string TransactionTypeEPO = "Emergency Purchase Order";
        public const string TransactionTypeSPO = "Service Purchase Order";
        public const string TransactionTypeRFQ = "RFQ";
        public const string TransactionTypeDutyMaster = "Duty Master";
		public const string TransactionTypeContract = "Purchase Contract";
        public const string TransactionTypePurchaseReturn = "Purchase Return";
        public const string TransactionTypeGRN = "GRN";
        public const string TransactionTypeVendorEvaluation = "Vendor Evaluation";
        public const string TransactionTypeOCR = "OCR";
        public const string ModulePurchase = "Purchase";

        // Purchase Return (RTV) — MiscMaster type codes (seeded via migration)
        public const string RtvStatus = "RtvStatus";
        public const string RtvReturnAction = "RtvReturnAction";
        public const string RtvReplacementStatus = "RtvReplacementStatus";
        public const string RtvInventoryImpact = "RtvInventoryImpact";
        public const string RtvFinanceImpact = "RtvFinanceImpact";

        // RTV status code values (also seeded under RtvStatus MiscType)
        public const string RtvPendingApproval = "PendingApproval";
        public const string RtvShipped = "Shipped";
        public const string RtvClosed = "Closed";

        // Approval workflow integration
        public const string RtvModuleTypeName = "Purchase Return";

        // Blanket PO constants
        public const string Blanket = "Blanket";
        public const string TransactionTypeBlanket = "Blanket Master";
        public const string TransactionTypeBPO = "Blanket Purchase Order";

        // Bale barcode series — MiscMaster type codes (seeded via SQL).
        // Default status value reuses the existing "Open" constant above.
        public const string BarcodePrefix = "BarcodePrefix";
        public const string BarcodeSeriesStatus = "BarcodeSeriesStatus";

        // Bale barcode series status values (seeded under BarcodeSeriesStatus). Default "Open" reuses const above.
        public const string BarcodeSeriesPartiallyAllocated = "PartiallyAllocated";
        public const string BarcodeSeriesFullyAllocated = "FullyAllocated";

        // Bale barcode allocation — MiscMaster type code + status values (seeded via SQL).
        public const string BarcodeAllocationStatus = "BarcodeAllocationStatus";
    }
}
