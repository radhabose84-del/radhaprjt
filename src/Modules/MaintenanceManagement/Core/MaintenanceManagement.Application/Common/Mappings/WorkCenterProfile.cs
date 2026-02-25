using AutoMapper;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class WorkCenterProfile : Profile
    {
        public WorkCenterProfile()
        {
              CreateMap<MaintenanceManagement.Domain.Entities.WorkCenter,WorkCenterDto>();
              CreateMap<MaintenanceManagement.Domain.Entities.WorkCenter, WorkCenterAutoCompleteDto>();
              CreateMap<CreateWorkCenterCommand, MaintenanceManagement.Domain.Entities.WorkCenter>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.WorkCenterCode, opt => opt.MapFrom(src => src.WorkCenterCode))
                .ForMember(dest => dest.WorkCenterName, opt => opt.MapFrom(src => src.WorkCenterName))
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

              CreateMap<UpdateWorkCenterCommand, MaintenanceManagement.Domain.Entities.WorkCenter>()
                .ForMember(dest => dest.WorkCenterCode, opt => opt.Ignore())
                .ForMember(dest => dest.WorkCenterName, opt => opt.MapFrom(src => src.WorkCenterName))
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

              CreateMap<DeleteWorkCenterCommand, MaintenanceManagement.Domain.Entities.WorkCenter>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));  
        }
    }
}