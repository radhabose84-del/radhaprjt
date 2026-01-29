using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MaintenanceRequestProfile : Profile
    {
        
        private readonly IIPAddressService _ipAddressService;

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
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => _ipAddressService.GetCompanyId()))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => _ipAddressService.GetUnitId()));


            CreateMap<GetExternalRequestByIdDto,MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestId,  opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.RequestStatusId))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId));
            
        }
    }
}