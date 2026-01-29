using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings.Power
{
    public class FeederProfile : Profile
    {

        public FeederProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.Power.Feeder, GetFeederDto>();
            CreateMap<MaintenanceManagement.Domain.Entities.Power.Feeder, GetFeederByIdDto>()                      
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active));

            CreateMap<CreateFeederCommand, MaintenanceManagement.Domain.Entities.Power.Feeder>()
             .ForMember(dest => dest.HighPriority, opt => opt.MapFrom(src => src.HighPriority == 1 ? true : false))
             .ForMember(dest => dest.MeterAvailable, opt => opt.MapFrom(src => src.MeterAvailable == 1));

            CreateMap<UpdateFeederCommand, MaintenanceManagement.Domain.Entities.Power.Feeder>()
                .ForMember(dest => dest.HighPriority, opt => opt.MapFrom(src => src.HighPriority == 1 ? true : false))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
             .ForMember(dest => dest.MeterAvailable, opt => opt.MapFrom(src => src.MeterAvailable == 1));

            CreateMap<DeleteFeederCommand, MaintenanceManagement.Domain.Entities.Power.Feeder>()
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
             
            CreateMap<MaintenanceManagement.Domain.Entities.Power.Feeder, GetFeederAutoCompleteDto>();

        }
        
    }
}