using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class ServiceMasterProfile : Profile
    {

        public ServiceMasterProfile()
        {
            CreateMap<PurchaseManagement.Domain.Entities.ServiceMaster, GetServiceMasterDto>();

            CreateMap<CreateServiceCommand, PurchaseManagement.Domain.Entities.ServiceMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateServiceCommand, PurchaseManagement.Domain.Entities.ServiceMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteServiceCommand, PurchaseManagement.Domain.Entities.ServiceMaster>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => Status.Inactive));
               
            CreateMap<PurchaseManagement.Domain.Entities.ServiceMaster, ServiceMasterAutoCompleteDto>();    

        }
        
    }
}