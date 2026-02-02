
using AutoMapper;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;


namespace PurchaseManagement.Application.Common.Mappings.PurchaseOrder;

public class PurchaseBillEntryProfile : Profile
{
    public PurchaseBillEntryProfile()
    {
        // Entity -> DTO
        CreateMap<PurchaseBillEntryHeader, PurchaseBillEntryHeaderDto>();
        CreateMap<PurchaseBillEntryDetail, PurchaseBillEntryDetailDto>();

        // DTO -> Entity
        CreateMap<PurchaseBillEntryHeaderDto, PurchaseBillEntryHeader>()
            .ForMember(d => d.Id, opt => opt.Ignore()) // we handle for update
            .ForMember(d => d.Lines, opt => opt.Ignore()); // lines manually
        CreateMap<PurchaseBillEntryDetailDto, PurchaseBillEntryDetail>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.BillEntryHeaderId, opt => opt.Ignore());
    }
}
