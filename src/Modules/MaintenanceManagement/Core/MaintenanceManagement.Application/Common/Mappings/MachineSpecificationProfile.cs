using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication;
using MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MachineSpecificationProfile : Profile
    {
        public MachineSpecificationProfile()
        {
            CreateMap<MaintenanceManagement.Domain.Entities.MachineSpecification, MachineSpecificationDto>();

            CreateMap<MachineSpecificationCreateDto, MaintenanceManagement.Domain.Entities.MachineSpecification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<DeleteMachineSpecficationCommand, MaintenanceManagement.Domain.Entities.MachineSpecification>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
                
             CreateMap<MachineSpecificationUpdateDto, MaintenanceManagement.Domain.Entities.MachineSpecification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

        }
    }
}