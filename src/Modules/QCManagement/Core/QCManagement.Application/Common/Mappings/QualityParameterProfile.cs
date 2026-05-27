using AutoMapper;
using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.Common.Mappings
{
    public class QualityParameterProfile : Profile
    {
        public QualityParameterProfile()
        {
            CreateMap<CreateQualityParameterCommand, Domain.Entities.QualityParameter>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.ParameterCode, opt => opt.Ignore());

            CreateMap<UpdateQualityParameterCommand, Domain.Entities.QualityParameter>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
