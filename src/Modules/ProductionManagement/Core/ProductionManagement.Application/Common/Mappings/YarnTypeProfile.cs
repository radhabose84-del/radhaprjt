using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Application.YarnType.Commands.UpdateYarnType;
using ProductionManagement.Application.YarnType.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class YarnTypeProfile : Profile
    {
        public YarnTypeProfile()
        {
            CreateMap<CreateYarnTypeCommand, Domain.Entities.YarnType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateYarnTypeCommand, Domain.Entities.YarnType>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.YarnType, YarnTypeDto>();
            CreateMap<YarnTypeDto, YarnTypeDto>();
            CreateMap<Domain.Entities.YarnType, YarnTypeLookupDto>();
            CreateMap<YarnTypeLookupDto, YarnTypeLookupDto>();
        }
    }
}
