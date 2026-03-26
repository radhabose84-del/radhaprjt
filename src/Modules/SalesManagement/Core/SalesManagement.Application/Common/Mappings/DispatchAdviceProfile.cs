using AutoMapper;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class DispatchAdviceProfile : Profile
    {
        public DispatchAdviceProfile()
        {
            // Create: Command → Header entity (with nested details)
            CreateMap<CreateDispatchAdviceCommand, DispatchAdviceHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DispatchNo, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.InvFlg, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.DispatchAdviceDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Detail input DTO → Detail entity
            CreateMap<CreateDispatchAdviceDetailDto, DispatchAdviceDetail>();

            // Entity → DTO (for query mapping)
            CreateMap<DispatchAdviceHeader, DispatchAdviceHeaderDto>();
            CreateMap<DispatchAdviceDetail, DispatchAdviceDetailDto>();
        }
    }
}
