using AutoMapper;
using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MachineGroupProfile : Profile
    {

       public MachineGroupProfile()
       {
            CreateMap<MaintenanceManagement.Domain.Entities.MachineGroup, MachineGroupDto>();

            CreateMap<CreateMachineGroupCommand, MaintenanceManagement.Domain.Entities.MachineGroup>()
            .ForMember(dest => dest.PowerSource, opt => opt.MapFrom(src => src.PowerSource == 1 ? true : false))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
             CreateMap<MaintenanceManagement.Domain.Entities.MachineGroup, GetMachineGroupByIdDto>();

            CreateMap<UpdateMachineGroupCommand, MaintenanceManagement.Domain.Entities.MachineGroup>()
            .ForMember(dest => dest.PowerSource, opt => opt.MapFrom(src => src.PowerSource == 1 ? true : false))
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));       

            CreateMap<DeleteMachineGroupCommand, MaintenanceManagement.Domain.Entities.MachineGroup>()           
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));     

            CreateMap<MaintenanceManagement.Domain.Entities.MachineGroup, GetMachineGroupAutoCompleteDto>();        

       }    
        
    }
}