using AutoMapper;
using MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MachineGroupUserProfile  : Profile
    {
       public MachineGroupUserProfile()
       {
            CreateMap<MaintenanceManagement.Domain.Entities.MachineGroupUser, MachineGroupUserDto>();

            CreateMap<CreateMachineGroupUserCommand, MaintenanceManagement.Domain.Entities.MachineGroupUser>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
             CreateMap<MaintenanceManagement.Domain.Entities.MachineGroupUser, MachineGroupUserDto>();

            CreateMap<UpdateMachineGroupUserCommand, MaintenanceManagement.Domain.Entities.MachineGroupUser>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));       

            CreateMap<DeleteMachineGroupUserCommand, MaintenanceManagement.Domain.Entities.MachineGroupUser>()           
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));     

            CreateMap<MaintenanceManagement.Domain.Entities.MachineGroupUser, MachineGroupUserAutoCompleteDto>();        
       }    
        
    }
}