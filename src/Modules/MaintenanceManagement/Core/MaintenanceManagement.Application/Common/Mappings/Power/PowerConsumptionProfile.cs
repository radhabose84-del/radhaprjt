using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings.Power
{
    public class PowerConsumptionProfile : Profile
    {
        public PowerConsumptionProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.Power.PowerConsumption, PowerConsumptionDto>();
            CreateMap<CreatePowerConsumptionCommand, MaintenanceManagement.Domain.Entities.Power.PowerConsumption>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TotalUnits, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}