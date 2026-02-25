using AutoMapper;
using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MaintenanceTypeProfile : Profile
    {
         public MaintenanceTypeProfile()
        {
           CreateMap<MaintenanceManagement.Domain.Entities.MaintenanceType,MaintenanceTypeDto>();
           CreateMap<MaintenanceManagement.Domain.Entities.MaintenanceType, MaintenanceTypeAutoCompleteDto>(); 
           CreateMap<CreateMaintenanceTypeCommand, MaintenanceManagement.Domain.Entities.MaintenanceType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateMaintenanceTypeCommand, MaintenanceManagement.Domain.Entities.MaintenanceType>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


              CreateMap<DeleteMaintenanceTypeCommand, MaintenanceManagement.Domain.Entities.MaintenanceType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }
    }
}