using AutoMapper;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class StoReceiptProfile : Profile
    {
        public StoReceiptProfile()
        {
            // Create: Command → Header entity (write-once, no update)
            CreateMap<CreateStoReceiptCommand, StoReceiptHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StoReceiptNumber, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.StoReceiptDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Detail input DTO → Detail entity
            CreateMap<CreateStoReceiptDetailDto, StoReceiptDetail>();

            // Entity → DTO (for query mapping)
            CreateMap<StoReceiptHeader, StoReceiptHeaderDto>();
            CreateMap<StoReceiptDetail, StoReceiptDetailDto>();
        }
    }
}
