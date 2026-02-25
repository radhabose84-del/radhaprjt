using AutoMapper;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MachineMasterProfile :Profile
    {
        public MachineMasterProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.MachineMaster, MachineMasterDto>();

            CreateMap<MaintenanceManagement.Domain.Entities.MachineMaster, MachineMasterAutoCompleteDto>()
             .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.MachineGroup.DepartmentId));
            CreateMap<CreateMachineMasterCommand, MaintenanceManagement.Domain.Entities.MachineMaster>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsProductionMachine, opt => opt.MapFrom(src => src.IsProductionMachine == 1 ? true : false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMachineMasterCommand, MaintenanceManagement.Domain.Entities.MachineMaster>()
                .ForMember(dest => dest.IsProductionMachine, opt => opt.MapFrom(src => src.IsProductionMachine == 1 ? true : false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

               CreateMap<DeleteMachineMasterCommand, MaintenanceManagement.Domain.Entities.MachineMaster>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 

            
        }
    }
}