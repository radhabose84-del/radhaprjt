using AutoMapper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Application.PurchaseOrder.Mapping
{
    public sealed class ImportPOProfile : Profile
    {
        public ImportPOProfile()
        {
            /* ===================== ROOT: CREATE ===================== */
            CreateMap<ImportPOCreateDto, PurchaseOrderHeader>()
                // server/workflow fields
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UnitId, o => o.Ignore())
                .ForMember(d => d.PONumber, o => o.Ignore())
                .ForMember(d => d.OldPOId, o => o.Ignore())
                .ForMember(d => d.StatusId, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreatedByName, o => o.Ignore())
                .ForMember(d => d.CreatedIP, o => o.Ignore())
                .ForMember(d => d.CreatedDate, o => o.Ignore())
                .ForMember(d => d.ModifiedDate, o => o.Ignore())
                // local headers are not part of import
                .ForMember(d => d.Headers, o => o.Ignore())
                // import children
                .ForMember(d => d.ImportPOHeader, o => o.MapFrom(s => s.Headers))
                .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
                // other navigations
                .ForMember(d => d.POGateEntriesDetails, o => o.Ignore())
                .ForMember(d => d.PoGrnDetails, o => o.Ignore())
                .ForMember(d => d.ServicePos, o => o.Ignore())
                .ForMember(d => d.ServiceEntrySheets, o => o.Ignore())
                .ForMember(d => d.PurchaseDocumentTypes, o => o.Ignore())
                // 0 -> NULL for optional FKs
                .ForMember(d => d.CapitalTypeId, o => o.MapFrom(s => s.CapitalTypeId > 0 ? (int?)s.CapitalTypeId : null))
                .ForMember(d => d.PurchaseTypeId, o => o.MapFrom(s => s.PurchaseTypeId > 0 ? (int?)s.PurchaseTypeId : null))
                .ForMember(d => d.CostCenterId, o => o.MapFrom(s => s.CostCenterId > 0 ? (int?)s.CostCenterId : null))
                .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId > 0 ? (int?)s.ProjectId : null));


            /* ===================== ROOT: UPDATE ===================== */
            CreateMap<ImportPOUpdateDto, PurchaseOrderHeader>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UnitId, o => o.Ignore())
                .ForMember(d => d.PONumber, o => o.Ignore())
                .ForMember(d => d.OldPOId, o => o.Ignore())
                .ForMember(d => d.StatusId, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreatedByName, o => o.Ignore())
                .ForMember(d => d.CreatedIP, o => o.Ignore())
                .ForMember(d => d.CreatedDate, o => o.Ignore())
                .ForMember(d => d.ModifiedDate, o => o.Ignore())
                .ForMember(d => d.Headers, o => o.Ignore())
                .ForMember(d => d.ImportPOHeader, o => o.MapFrom(s => s.Headers))
                .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
                .ForMember(d => d.POGateEntriesDetails, o => o.Ignore())
                .ForMember(d => d.PoGrnDetails, o => o.Ignore())
                .ForMember(d => d.ServicePos, o => o.Ignore())
                .ForMember(d => d.ServiceEntrySheets, o => o.Ignore())
                .ForMember(d => d.PurchaseDocumentTypes, o => o.Ignore())
                .ForMember(d => d.CapitalTypeId, o => o.MapFrom(s => s.CapitalTypeId > 0 ? (int?)s.CapitalTypeId : null))
                .ForMember(d => d.PurchaseTypeId, o => o.MapFrom(s => s.PurchaseTypeId > 0 ? (int?)s.PurchaseTypeId : null))
                .ForMember(d => d.CostCenterId, o => o.MapFrom(s => s.CostCenterId > 0 ? (int?)s.CostCenterId : null))
                .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId > 0 ? (int?)s.ProjectId : null));

            /* ===================== CHILD: HEADER ===================== */
            CreateMap<ImportPOHeaderDto, ImportPOHeader>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PurchaseOrderId, o => o.Ignore())  
                .ForMember(d => d.ImportPurchase, o => o.Ignore())
                .ForMember(d => d.EXRate, o => o.Ignore())
                .ForMember(d => d.MiscIncoterms, o => o.Ignore())
                .ForMember(d => d.ShipPort, o => o.Ignore())
                .ForMember(d => d.DestPort, o => o.Ignore())
                .ForMember(d => d.MOT, o => o.Ignore())
                .ForMember(d => d.CustomsOffice, o => o.Ignore())
                .ForMember(d => d.LCPaymentMode, o => o.Ignore())
                .ForMember(d => d.LCType, o => o.Ignore())
                .ForMember(d => d.LCPaymentStatus, o => o.Ignore())                
                .ForMember(d => d.ImportPODetails, o => o.MapFrom(s => s.Details))
                .ForAllMembers(o => o.Condition((_, __, src) => src != null));

            /* ===================== CHILD: DETAIL ===================== */
            CreateMap<ImportPODetailDto, ImportPODetail>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PurchaseHeaderId, o => o.Ignore())  // set by parent
                .ForMember(d => d.Header, o => o.Ignore())
                .ForMember(d => d.dutyMaster, o => o.Ignore())
                .ForMember(d => d.GRBasedIV, o => o.MapFrom(s => s.GRBasedIV))
                .ForAllMembers(o => o.Condition((_, __, src) => src != null));

            /* ===================== CHILD: TERMS ===================== */
            CreateMap<ImportPurchasePaymentTermDto, PurchasePaymentTerm>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PurchaseOrderId, o => o.Ignore())  // set in repo
                .ForAllMembers(o => o.Condition((_, __, src) => src != null));

            /* ===================== DOCS ===================== */
            CreateMap<DocumentDto, PurchaseDocument>()            
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PoId, o => o.Ignore()); // set in repo
                
         CreateMap<ImportPOFullVm, CreateImportPOReverseDto>()
    .ConvertUsing((src, ctx) =>
    {
        
        var header = new ImportPOWorkFlowDto
        {
            Id       = src.PO.Id,
            PONumber = src.PO.PONumber,
            VendorId = src.PO.VendorId,
            StatusId = src.PO.StatusId,
            UnitId   =  src.PO.UnitId
        };

        var lines = (src.ImportHeaders ?? new List<ImportPOHeaderReadDto>())
            .Select(h => new ImportPOWorkFlowDto
            {
                Id       = h.Id,
                PONumber = src.PO.PONumber,
                VendorId = src.PO.VendorId,
                StatusId = src.PO.StatusId,
                UnitId   =  src.PO.UnitId
            })
            .ToList();

        return new CreateImportPOReverseDto { Header = header, Lines = lines };
    });

        }
    }
}
