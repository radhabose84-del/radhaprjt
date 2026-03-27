using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class CertificationMasterProfile : Profile
    {
        public CertificationMasterProfile()
        {
            CreateMap<CreateCertificationMasterCommand, Domain.Entities.CertificationMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateCertificationMasterCommand, Domain.Entities.CertificationMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.CertificationMaster, CertificationMasterDto>();
            CreateMap<CertificationMasterDto, CertificationMasterDto>();
            CreateMap<Domain.Entities.CertificationMaster, CertificationMasterLookupDto>();
            CreateMap<CertificationMasterLookupDto, CertificationMasterLookupDto>();
        }
    }
}
