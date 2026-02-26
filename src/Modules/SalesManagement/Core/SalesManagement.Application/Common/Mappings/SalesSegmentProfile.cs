using AutoMapper;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesSegmentProfile : Profile
    {
        public SalesSegmentProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.SalesSegment, SalesSegmentDto>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            // Entity to Lookup DTO
            CreateMap<Domain.Entities.SalesSegment, SalesSegmentLookupDto>();

            // Command to Entity
            CreateMap<CreateSalesSegmentCommand, Domain.Entities.SalesSegment>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesSegmentCommand, Domain.Entities.SalesSegment>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
