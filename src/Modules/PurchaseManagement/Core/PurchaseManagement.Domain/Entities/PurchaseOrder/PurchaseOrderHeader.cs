using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder;

public class PurchaseOrderHeader : BaseEntity, IActivityTracked
{
    public int UnitId { get; set; }
    public string PONumber { get; set; } = default!;
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public MiscMaster? MiscPoCategory { get; set; }
    public int? POMethodId { get; set; }
    public MiscMaster? MiscPoMethod { get; set; }
    public int CurrencyId { get; set; }
    public int VendorId { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? PandFTotal { get; set; }
    public decimal? MiscCharges { get; set; }    
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal? FreightTotal { get; set; }
    public decimal? InsuranceTotal { get; set; }
    public decimal? TDSTotal { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public int? OldPOId { get; set; }      
    public int RevisionNo { get; set; } = 0; 
    public string? AmendmentReason { get; set; }
    public int? CapitalTypeId { get; set; }  
    public MiscMaster? MiscCapitalType { get; set; }
    public int? PurchaseTypeId { get; set; }
    public MiscMaster? MiscPurchaseType { get; set; }
    public int? ProjectId { get; set; } 
    public int? WBSId { get; set; }
    public int? CostCenterId { get; set; } 
    public int? BudgetGroupId { get; set; }
    public int? ItemCategoryId { get; set; }
    public int? BudgetRequestById { get; set; } 
    public int? BudgetDepartmentId { get; set; } 
    public int? FinancialYearId { get; set; }
    public int? BudgetMonthId { get; set; }
    public ICollection<PurchaseLocalHeader> Headers { get; set; } = new List<PurchaseLocalHeader>();
    public ICollection<PurchasePaymentTerm> PaymentTerms { get; set; } = new List<PurchasePaymentTerm>();
    public ICollection<GateEntryDetail> POGateEntriesDetails { get; set; } = new List<GateEntryDetail>();
    public ICollection<GrnDetail> PoGrnDetails { get; set; } = new List<GrnDetail>();
    public ICollection<PurchaseOrderServiceHeader> ServicePos { get; set; } = new List<PurchaseOrderServiceHeader>();
    public ICollection<ServiceEntrySheet> ServiceEntrySheets { get; set; } = new List<ServiceEntrySheet>();
    public ICollection<ImportPOHeader> ImportPOHeader { get; set; } = new List<ImportPOHeader>();

    // Contract PO (4th PO type — release PO against a contract)
    public ICollection<PurchaseContractHeader> ContractPOHeaders { get; set; } = new List<PurchaseContractHeader>();

    // Release history — tracks which release POs were created against contracts
    public ICollection<ContractPOReleaseHistory> ContractPOReleaseHistories { get; set; } = new List<ContractPOReleaseHistory>();

    // Cancelled fields
    public DateTimeOffset? CancelledDate { get; set; }
    public string? CancelledByName { get; set; }
    public string? CancelledIP { get; set; }

    // ForeClosed fields
    public DateTimeOffset? ForeClosedDate { get; set; }
    public string? ForeClosedByName { get; set; }
    public string? ForeClosedIP { get; set; }

    public ICollection<PurchaseDocument>? PurchaseDocumentTypes { get; set; }= new List<PurchaseDocument>();
}


