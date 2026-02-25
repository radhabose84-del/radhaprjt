using AutoMapper;
using MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;


namespace MaintenanceManagement.Application.Common.Mappings
{
    public class ActivityMasterProfile :Profile
    {
        public ActivityMasterProfile()
        {
           
           
            CreateMap<MaintenanceManagement.Domain.Entities.ActivityMaster, GetAllActivityMasterDto>(); 
             CreateMap<ActivityMachineGroup, GetAllMachineGroupDto>();          

            CreateMap<MaintenanceManagement.Domain.Entities.ActivityMaster, GetActivityMasterByIdDto>();

            CreateMap<MaintenanceManagement.Domain.Entities.ActivityMaster, GetActivityMasterAutoCompleteDto>();

            CreateMap<CreateActivityMasterDto, MaintenanceManagement.Domain.Entities.ActivityMaster>()
            .ForMember(dest => dest.ActivityMachineGroups, opt => opt.MapFrom(src => src.ActivityMachineGroup)) 
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
            CreateMap<ActivityMachineGroupDto, ActivityMachineGroup>();

            CreateMap<UpdateActivityMasterDto, MaintenanceManagement.Domain.Entities.ActivityMaster>()
           .ForMember(dest => dest.ActivityMachineGroups, opt => opt.MapFrom(src => src.UpdateActivityMachineGroup));
            
            CreateMap<UpdateActivityMachineGroupDto, ActivityMachineGroup>();

           


           

         



        }


    }
}