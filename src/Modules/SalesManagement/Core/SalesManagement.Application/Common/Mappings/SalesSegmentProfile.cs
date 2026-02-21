#nullable disable

using AutoMapper;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesSegmentProfile : Profile
    {
        public SalesSegmentProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.SalesSegment, SalesSegmentDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Domain.Common.BaseEntity.Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == Domain.Common.BaseEntity.IsDelete.Deleted));

            // Entity to Lookup DTO
            CreateMap<Domain.Entities.SalesSegment, SalesSegmentLookupDto>();

            // Command to Entity
            CreateMap<CreateSalesSegmentCommand, Domain.Entities.SalesSegment>();
            CreateMap<UpdateSalesSegmentCommand, Domain.Entities.SalesSegment>();
        }
    }
}
