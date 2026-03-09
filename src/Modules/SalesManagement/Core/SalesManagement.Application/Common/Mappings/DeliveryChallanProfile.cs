using AutoMapper;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class DeliveryChallanProfile : Profile
    {
        public DeliveryChallanProfile()
        {
            // Create: Command → Header entity (write-once, no update)
            CreateMap<DeliveryChallan.Commands.CreateDeliveryChallan.CreateDeliveryChallanCommand, DeliveryChallanHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryNumber, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryValue, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryChallanDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Detail input DTO → Detail entity
            CreateMap<CreateDeliveryChallanDetailDto, DeliveryChallanDetail>()
                .ForMember(dest => dest.LineMovementValue, opt => opt.MapFrom(src => src.DispatchQuantity * src.ExMillRate));

            // Entity → DTO (for query mapping)
            CreateMap<DeliveryChallanHeader, DeliveryChallanHeaderDto>();
            CreateMap<DeliveryChallanDetail, DeliveryChallanDetailDto>();
        }
    }
}
