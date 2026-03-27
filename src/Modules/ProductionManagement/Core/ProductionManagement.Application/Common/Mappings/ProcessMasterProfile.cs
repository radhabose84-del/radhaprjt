using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.ProcessMaster.Commands.CreateProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.UpdateProcessMaster;
using ProductionManagement.Application.ProcessMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class ProcessMasterProfile : Profile
    {
        public ProcessMasterProfile()
        {
            CreateMap<CreateProcessMasterCommand, Domain.Entities.ProcessMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.CombingRequired, opt => opt.MapFrom(src => src.CombingRequired));

            CreateMap<UpdateProcessMasterCommand, Domain.Entities.ProcessMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.ProcessMaster, ProcessMasterDto>();
            CreateMap<ProcessMasterDto, ProcessMasterDto>();
            CreateMap<Domain.Entities.ProcessMaster, ProcessMasterLookupDto>();
            CreateMap<ProcessMasterLookupDto, ProcessMasterLookupDto>();
        }
    }
}
