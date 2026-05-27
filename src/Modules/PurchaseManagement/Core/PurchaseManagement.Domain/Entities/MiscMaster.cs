using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
namespace PurchaseManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public MiscTypeMaster? MiscTypeMaster { get; set; }
        public ICollection<IndentHeader> IndentType { get; set; } = default!;
        public ICollection<PaymentTermMaster>? PaymentTermsAsBaselineType { get; set; }
        public ICollection<IndentDetail> StatusDetail { get; set; } = default!;
        public ICollection<RfqMaster> RfqStatuses { get; set; } = new List<RfqMaster>();
        public ICollection<RfqMaster> RfqInitiationTypes { get; set; } = new List<RfqMaster>();
        public ICollection<IndentHeader> StatusHeader { get; set; } = default!;
        public ICollection<TnCTemplateMaster> TncTemplatesByType { get; set; } = new List<TnCTemplateMaster>();
        public ICollection<TnCTemplateApplicability> TncApplicabilities { get; set; } = new List<TnCTemplateApplicability>();
        //Quotation Entry
        public ICollection<QuotationHeader> QuotationPaymentTerms { get; set; } = new List<QuotationHeader>();
        public ICollection<QuotationHeader> QuotationFreightMode { get; set; } = new List<QuotationHeader>();
        public ICollection<QuotationHeader> QuotationIncoterms { get; set; } = new List<QuotationHeader>();
        public ICollection<QuotationComparisonHeader>? StatusWorkflow { get; set; }

        public ICollection<PriceMasterHeader>? PriceMasterSourceFrom { get; set; }
        public ICollection<PriceMasterHeader>? PriceMasterStatus { get; set; }
        public ICollection<GateEntryDetail>? PoTypeGateEntry { get; set; }
        public ICollection<GateEntryHeader>? GateEntryReceived { get; set; }

        public ICollection<PurchaseOrderHeader>? PurchaseOrderCategory { get; set; }
        public ICollection<PurchaseOrderHeader>? PurchaseOrderMethod { get; set; }
        public ICollection<PurchasePaymentTerm>? PurchaseOrderPaymentTerms { get; set; }
        public ICollection<PurchasePaymentTerm>? PurchaseOrderPaymentMode { get; set; }
        public ICollection<PurchaseLocalDetail>? PurchaseLocalDetailDiscount { get; set; }
        public ICollection<PurchaseLocalHeader>? PurchaseLocalHeaderIncoterms { get; set; }
        public ICollection<PurchaseLocalHeader>? PurchaseLocalHeaderMode { get; set; }
        public ICollection<GrnHeader>? GrnQcStatusMisc { get; set; }
        public ICollection<GrnDetail>? GrnDetailsPoCategory { get; set; }
        public ICollection<GrnDetail>? GrnDetailsPoMethod { get; set; }
        public ICollection<GateEntryDetail>? GateEntryDetailsPoMethod { get; set; }

        public ICollection<PurchaseOrderServiceHeader>? PurchaseOrderServiceHeaderServiceCategories { get; set; }

        public ICollection<PurchaseOrderServiceHeader>? PurchaseOrderServiceHeaderContractTypes { get; set; }
        public ICollection<PurchaseOrderServiceHeader>? PurchaseOrderServiceHeaderFrequencies { get; set; }



        public ICollection<MrsHeader>? MrsDetailsHeader { get; set; }
        public ICollection<MrsHeader>? MrsRequestHeader { get; set; }
        public ICollection<QuotationDetail>? QuotationDetailDiscount { get; set; }

        public ICollection<PortMaster>? PortType { get; set; }
        public ICollection<PortMaster>? PortMiscType { get; set; }

        //public ICollection<ServiceEntrySheet> ServiceEntrySheets { get; set; } = new List<ServiceEntrySheet>();

        public ICollection<ServiceEntryActivity> ActivityTypes { get; set; } = new List<ServiceEntryActivity>(); // for ActivityType
        public ICollection<ServiceEntryActivity> SESActivityStatuses { get; set; } = new List<ServiceEntryActivity>(); // for SESActivityStatus

        public ICollection<ImportPOHeader>? importPOHeaderIncoterms { get; set; }
        public ICollection<ImportPOHeader>? importPOHeaderMOT { get; set; }
        public ICollection<ImportPOHeader>? importPOHeaderShipMode { get; set; }
        public ICollection<ImportPOHeader>? importPOHeaderCustomsOffice { get; set; }
        public ICollection<DutyMaster>? dutyCategory { get; set; }
        public ICollection<DutyMaster>? dutyCOA { get; set; }
        public ICollection<IssueReturnHeader>? IssueReturnMiscRequestHeader { get; set; }
        public ICollection<IssueReturnDetail>? IssueReturnDetailMiscRequestHeader { get; set; }
        public ICollection<IssueReturnDetail>? IssueReturnDetailsReasonMisc { get; set; }
        public ICollection<PurchaseOrderServiceHeader>? PurchaseServiceHeaderMode { get; set; }
        public ICollection<ImportPOHeader>? ImportLCPayment { get; set; }
        public ICollection<ImportPOHeader>? ImportLCType { get; set; }
        public ICollection<ImportPOHeader>? ImportTTPayment { get; set; }
        public ICollection<ImportPOHeader>? ImportLCPaymentMode { get; set; }
        public ICollection<ImportPOHeader>? ImportTTPaymentMode { get; set; }
        public ICollection<PurchaseOrderHeader>? POCapitalType { get; set; }
        public ICollection<PurchaseOrderHeader>? POPurchaseType { get; set; }

        // Contract PO — standalone contract status
        public ICollection<ContractPOHeader>? ContractPOStatuses { get; set; }

        // Purchase Contract Header — Incoterms & ModeOfDispatch (4th PO type)
        public ICollection<PurchaseContractHeader>? PurchaseContractHeaderIncoterms { get; set; }
        public ICollection<PurchaseContractHeader>? PurchaseContractHeaderMode { get; set; }

        // Purchase Contract Detail — Discount type
        public ICollection<PurchaseContractDetail>? PurchaseContractDetailDiscount { get; set; }

        // Vendor Evaluation — Scoring Method & Rating Impact
        public ICollection<VendorEvaluationCriteria>? VendorCriteriaScoringMethod { get; set; }
        public ICollection<VendorEvaluationCriteria>? VendorCriteriaRatingImpact { get; set; }

        // Vendor Evaluation — Status
        public ICollection<VendorEvaluationHeader>? VendorEvaluationStatuses { get; set; }

        // Vendor Rating Grade — Action Type
        public ICollection<VendorRatingGrade>? VendorRatingGradeActions { get; set; }

    }
    }
        
    
