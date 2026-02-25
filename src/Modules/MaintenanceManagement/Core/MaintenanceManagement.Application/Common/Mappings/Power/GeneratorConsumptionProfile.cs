using AutoMapper;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Command;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings.Power
{
    public class GeneratorConsumptionProfile : Profile
    {
        public GeneratorConsumptionProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption, GeneratorConsumptionDto>();
            CreateMap<CreateGeneratorConsumptionCommand, MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Energy, opt => opt.Ignore())
                .ForMember(dest => dest.RunningHours, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}