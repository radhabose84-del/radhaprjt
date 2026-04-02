using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;
using ProductionManagement.Application.CountMaster.Commands.UpdateCountMaster;
using ProductionManagement.Application.CountMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class CountMasterProfile : Profile
    {
        public CountMasterProfile()
        {
            CreateMap<CreateCountMasterCommand, Domain.Entities.CountMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateCountMasterCommand, Domain.Entities.CountMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.CountMaster, CountMasterDto>();
            CreateMap<CountMasterDto, CountMasterDto>();
            CreateMap<Domain.Entities.CountMaster, CountMasterLookupDto>();
            CreateMap<CountMasterLookupDto, CountMasterLookupDto>();
        }
    }
}
