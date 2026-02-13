using AutoMapper;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MaintenanceRequestProfile : Profile
    {
        public MaintenanceRequestProfile()
        {
            CreateMap<CreateMaintenanceRequestCommand, MaintenanceManagement.Domain.Entities.MaintenanceRequest>();
            CreateMap<MaintenanceManagement.Domain.Entities.MaintenanceRequest,GetMaintenanceRequestDto>();


           CreateMap<UpdateMaintenanceRequestCommand, MaintenanceManagement.Domain.Entities.MaintenanceRequest>();

            CreateMap<MaintenanceManagement.Domain.Entities.ExistingVendorDetails, GetExistingVendorDetailsDto>();
            CreateMap<MaintenanceManagement.Domain.Entities.MaintenanceRequest, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.RequestStatusId))
            // Ignore navigation properties to prevent circular reference / stack overflow
            .ForMember(dest => dest.WOMaintenanceRequest, opt => opt.Ignore())
            .ForMember(dest => dest.WOPreventiveScheduler, opt => opt.Ignore())
            .ForMember(dest => dest.MiscStatus, opt => opt.Ignore())
            .ForMember(dest => dest.MiscRootCause, opt => opt.Ignore())
            // Ignore collections
            .ForMember(dest => dest.WorkOrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.WorkOrderActivities, opt => opt.Ignore())
            .ForMember(dest => dest.WorkOrderSchedules, opt => opt.Ignore())
            .ForMember(dest => dest.WorkOrderTechnicians, opt => opt.Ignore())
            .ForMember(dest => dest.WorkOrderCheckLists, opt => opt.Ignore());


            CreateMap<GetExternalRequestByIdDto,MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestId,  opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.RequestStatusId))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId));
            
        }
    }
}