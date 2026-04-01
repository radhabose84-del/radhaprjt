using AutoMapper;
using ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class YarnConversionHeaderProfile : Profile
    {
        public YarnConversionHeaderProfile()
        {
            CreateMap<CreateYarnConversionHeaderCommand, Domain.Entities.YarnConversionHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateYarnConversionHeaderCommand, Domain.Entities.YarnConversionHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
