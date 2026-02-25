using AutoMapper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class ActivityCheckListMasterProfile : Profile
    {
        public ActivityCheckListMasterProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster, GetAllActivityCheckListMasterDto>();           

            CreateMap<CreateActivityCheckListMasterCommand, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>();

            CreateMap<UpdateActivityCheckListMasterCommand, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));            
        
            CreateMap<DeleteActivityCheckListMasterCommand, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));                      


        }
        
    }
}