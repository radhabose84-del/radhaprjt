using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster;
using ProductionManagement.Application.QualityMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class QualityMasterProfile : Profile
    {
        public QualityMasterProfile()
        {
            CreateMap<CreateQualityMasterCommand, Domain.Entities.QualityMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateQualityMasterCommand, Domain.Entities.QualityMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.QualityMaster, QualityMasterDto>();
            CreateMap<QualityMasterDto, QualityMasterDto>();
            CreateMap<Domain.Entities.QualityMaster, QualityMasterLookupDto>();
            CreateMap<QualityMasterLookupDto, QualityMasterLookupDto>();
        }
    }
}
