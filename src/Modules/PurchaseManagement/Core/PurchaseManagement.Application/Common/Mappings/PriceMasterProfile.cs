using AutoMapper;
using PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Domain.Entities.PriceMaster;

namespace PurchaseManagement.Application.PriceMaster.Mapping
{
    public sealed class PriceMasterProfile : Profile
    {
        public PriceMasterProfile()
        {
            CreateMap<PriceMasterCreateDto, PriceMasterHeader>()
                .ForMember(d => d.Details, o => o.Ignore());

            CreateMap<PriceMasterUpdateDto, PriceMasterHeader>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Details, o => o.Ignore());

            CreateMap<PriceMasterDetailUpsertDto, PriceMasterDetail>();
            
            CreateMap<PriceMasterHeader, CreatePriceMasterReverseDto>()
            // Header
            .ForMember(d => d.Header, m => m.MapFrom(src => new PriceMaserWorkFlowDto
            {
                Id       = src.Id,
                ItemId   = src.ItemId.ToString(), // your DTO says string?
                VendorId = src.VendorId,
                StatusId = src.StatusId,
                UnitId   = src.UnitId
            }))
            // Lines (one per detail; you only asked for basic identity fields, same shape as Header)
            .ForMember(d => d.Lines, m => m.MapFrom(src =>
                (src.Details ?? Enumerable.Empty<PriceMasterDetail>()).Select(det => new PriceMaserWorkFlowDto
                {
                    Id       = det.Id,
                    ItemId   = src.ItemId.ToString(), // stays from header
                    VendorId = src.VendorId,
                    StatusId = src.StatusId,
                    UnitId   = src.UnitId
                })));
        }
    }
}
