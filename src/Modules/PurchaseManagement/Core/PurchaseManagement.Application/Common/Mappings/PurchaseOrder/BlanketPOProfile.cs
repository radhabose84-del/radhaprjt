using AutoMapper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseOrder;

public sealed class BlanketPOProfile : Profile
{
    public BlanketPOProfile()
    {
        // ── DTO → Entity (Create) ──────────────────────────────────────

        // BlanketPOCreateDto → PurchaseOrderHeader
        CreateMap<BlanketPOCreateDto, PurchaseOrderHeader>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PONumber, o => o.Ignore())
            .ForMember(d => d.CurrencyId, o => o.Ignore())
            .ForMember(d => d.VendorId, o => o.Ignore())
            .ForMember(d => d.RevisionNo, o => o.Ignore())
            .ForMember(d => d.OldPOId, o => o.Ignore())
            .ForMember(d => d.AmendmentReason, o => o.Ignore())
            .ForMember(d => d.CapitalTypeId, o => o.Ignore())
            .ForMember(d => d.PurchaseTypeId, o => o.Ignore())
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.WBSId, o => o.Ignore())
            .ForMember(d => d.CostCenterId, o => o.Ignore())
            .ForMember(d => d.BudgetGroupId, o => o.Ignore())
            .ForMember(d => d.ItemCategoryId, o => o.Ignore())
            .ForMember(d => d.BudgetRequestById, o => o.Ignore())
            .ForMember(d => d.BudgetDepartmentId, o => o.Ignore())
            .ForMember(d => d.FinancialYearId, o => o.Ignore())
            .ForMember(d => d.BudgetMonthId, o => o.Ignore())
            .ForMember(d => d.InsuranceTotal, o => o.Ignore())
            .ForMember(d => d.TDSTotal, o => o.Ignore())
            .ForMember(d => d.AdvanceAmount, o => o.Ignore())
            .ForMember(d => d.Headers, o => o.Ignore())
            .ForMember(d => d.PaymentTerms, o => o.Ignore())
            .ForMember(d => d.POGateEntriesDetails, o => o.Ignore())
            .ForMember(d => d.PoGrnDetails, o => o.Ignore())
            .ForMember(d => d.ServicePos, o => o.Ignore())
            .ForMember(d => d.ServiceEntrySheets, o => o.Ignore())
            .ForMember(d => d.ImportPOHeader, o => o.Ignore())
            .ForMember(d => d.ContractPOHeaders, o => o.Ignore())
            .ForMember(d => d.ContractPOReleaseHistories, o => o.Ignore())
            .ForMember(d => d.BlanketPOHeaders, o => o.Ignore())
            .ForMember(d => d.CancelledDate, o => o.Ignore())
            .ForMember(d => d.CancelledByName, o => o.Ignore())
            .ForMember(d => d.CancelledIP, o => o.Ignore())
            .ForMember(d => d.ForeClosedDate, o => o.Ignore())
            .ForMember(d => d.ForeClosedByName, o => o.Ignore())
            .ForMember(d => d.ForeClosedIP, o => o.Ignore())
            .ForMember(d => d.PurchaseDocumentTypes, o => o.Ignore())
            .ForMember(d => d.MiscPoCategory, o => o.Ignore())
            .ForMember(d => d.MiscPoMethod, o => o.Ignore())
            .ForMember(d => d.MiscCapitalType, o => o.Ignore())
            .ForMember(d => d.MiscPurchaseType, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        // BlanketPOCreateDto → PurchaseBlanketHeader
        CreateMap<BlanketPOCreateDto, PurchaseBlanketHeader>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseOrderId, o => o.Ignore())
            .ForMember(d => d.PurchaseOrder, o => o.Ignore())
            .ForMember(d => d.BlanketHeader, o => o.Ignore())
            .ForMember(d => d.MiscIncoterms, o => o.Ignore())
            .ForMember(d => d.MiscModeOfDispatch, o => o.Ignore())
            .ForMember(d => d.Details, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        // BlanketPODetailItem → PurchaseBlanketDetail
        CreateMap<BlanketPODetailItem, PurchaseBlanketDetail>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PurchaseBlanketHeaderId, o => o.Ignore())
            .ForMember(d => d.PurchaseBlanketHeader, o => o.Ignore())
            .ForMember(d => d.BlanketDetail, o => o.Ignore())
            .ForMember(d => d.MiscDiscountType, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        // ── Entity → DTO (read-back) ───────────────────────────────────

        CreateMap<PurchaseBlanketHeader, BlanketPOCreateDto>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details));

        CreateMap<PurchaseBlanketDetail, BlanketPODetailItem>();
    }
}
