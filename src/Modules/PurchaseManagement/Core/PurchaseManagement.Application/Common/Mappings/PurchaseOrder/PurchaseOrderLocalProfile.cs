using AutoMapper;
using LocalDtos = PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
// using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Application.Mappings.PurchaseOrder;

public class PurchaseOrderLocalProfile : Profile
{
    public PurchaseOrderLocalProfile()
    {
        // ----- DTO -> Entity
        CreateMap<LocalDtos.PurchaseLocalDetailDto, PurchaseLocalDetail>();
        CreateMap<LocalDtos.PurchaseLocalHeaderDto, PurchaseLocalHeader>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details));

        CreateMap<LocalDtos.PurchasePaymentTermDto, PurchasePaymentTerm>();
        CreateMap<LocalDtos.PurchaseDocumentDto, PurchaseDocument>().ReverseMap(); // for docs

        CreateMap<LocalDtos.PurchaseOrderCreateDto, PurchaseOrderHeader>()
            .ForMember(d => d.Headers, o => o.MapFrom(s => s.Headers))
            .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
            .ForMember(d => d.PurchaseDocumentTypes, o => o.MapFrom(s => s.Documents));

        CreateMap<LocalDtos.PurchaseOrderCreateDto, PurchaseOrderHeader>()
    .ForMember(d => d.CapitalTypeId, o => o.MapFrom(s => s.CapitalTypeId > 0 ? s.CapitalTypeId : (int?)null))
    .ForMember(d => d.PurchaseTypeId, o => o.MapFrom(s => s.PurchaseTypeId > 0 ? s.PurchaseTypeId : (int?)null))
    .ForMember(d => d.CostCenterId, o => o.MapFrom(s => s.CostCenterId > 0 ? s.CostCenterId : (int?)null))
    .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.ProjectId > 0 ? s.ProjectId : (int?)null));


        CreateMap<LocalDtos.PurchaseOrderUpdateDto, PurchaseOrderHeader>()
            .ForMember(d => d.Headers, o => o.MapFrom(s => s.Headers))
            .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms))
            .ForMember(d => d.PurchaseDocumentTypes, o => o.MapFrom(s => s.Documents));

        // ----- Entity -> DTO (read/detail)
        CreateMap<PurchaseLocalDetail, LocalDtos.PurchaseLocalDetailDto>();
        CreateMap<PurchaseLocalHeader, LocalDtos.PurchaseLocalHeaderDto>()
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details));
        CreateMap<PurchasePaymentTerm, LocalDtos.PurchasePaymentTermDto>();

        CreateMap<PurchaseOrderHeader, LocalDtos.PurchaseOrderDetailDto>()
            .ForMember(d => d.Headers, o => o.MapFrom(s => s.Headers))
            .ForMember(d => d.PaymentTerms, o => o.MapFrom(s => s.PaymentTerms));
        // NOTE: PurchaseOrderDetailDto currently has no Documents list.
        // If you need to return documents in "GetById", add to DTO:
        //   public List<PurchaseDocumentDto> Documents { get; set; } = new();
        // And map:
        //   .ForMember(d => d.Documents, o => o.MapFrom(s => s.PurchaseDocumentTypes));

        // ----- Workflow reverse payload (as you had)
        CreateMap<LocalDtos.PurchaseOrderDetailDto, CreatePOLocalReverseDto>()
            .ForMember(d => d.Header, o => o.MapFrom(s => new POLocalWorkFlowDto
            {
                Id = s.Id,
                PONumber = s.PONumber,
                VendorId = s.VendorId,
                StatusId = s.StatusId,
                UnitId = s.UnitId
            }))
            .ForMember(d => d.Lines, o => o.MapFrom(s =>
                (s.Headers ?? new List<LocalDtos.PurchaseLocalHeaderDto>())
                    .Where(h => h.Id.HasValue)
                    .Select(h => new POLocalWorkFlowDto
                    {
                        Id = h.Id!.Value,
                        PONumber = s.PONumber,
                        VendorId = s.VendorId,
                        StatusId = s.StatusId,
                        UnitId = s.UnitId
                    })
                    .ToList()
            ));         
    }
}
