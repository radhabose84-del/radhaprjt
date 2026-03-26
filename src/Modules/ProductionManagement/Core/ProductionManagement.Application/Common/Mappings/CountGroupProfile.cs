using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.CountGroup.Commands.CreateCountGroup;
using ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup;
using ProductionManagement.Application.CountGroup.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class CountGroupProfile : Profile
    {
        public CountGroupProfile()
        {
            CreateMap<CreateCountGroupCommand, Domain.Entities.CountGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateCountGroupCommand, Domain.Entities.CountGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.CountGroup, CountGroupDto>();
            CreateMap<CountGroupDto, CountGroupDto>();
            CreateMap<Domain.Entities.CountGroup, CountGroupLookupDto>();
            CreateMap<CountGroupLookupDto, CountGroupLookupDto>();
        }
    }
}
