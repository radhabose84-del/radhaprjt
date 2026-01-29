using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class ShiftMasterProfile : Profile
    {
        public ShiftMasterProfile()
        {
            CreateMap<CreateShiftMasterCommand, MaintenanceManagement.Domain.Entities.ShiftMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
            CreateMap<UpdateShiftMasterCommand, MaintenanceManagement.Domain.Entities.ShiftMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteShiftMasterCommand, MaintenanceManagement.Domain.Entities.ShiftMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            
            CreateMap<MaintenanceManagement.Domain.Entities.ShiftMaster, ShiftMasterDTO>();
            CreateMap<MaintenanceManagement.Domain.Entities.ShiftMaster, ShiftMasterAutoCompleteDTO>();
        }
    }
}