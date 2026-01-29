using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings.Power
{
    public class FeederGroupProfile : Profile
    {


        public FeederGroupProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.Power.FeederGroup, FeederGroupDto>();
            CreateMap<MaintenanceManagement.Domain.Entities.Power.FeederGroup, GetFeederGroupByIdDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active));

            CreateMap<MaintenanceManagement.Domain.Entities.Power.FeederGroup, GetFeederGroupAutoCompleteDto>();

            CreateMap<CreateFeederGroupCommand, MaintenanceManagement.Domain.Entities.Power.FeederGroup>();
            CreateMap<UpdateFeederGroupCommand, MaintenanceManagement.Domain.Entities.Power.FeederGroup>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (Status)src.IsActive));

            CreateMap<DeleteFeederGroupCommand, MaintenanceManagement.Domain.Entities.Power.FeederGroup>()
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
             
             
        }
        
    }
}