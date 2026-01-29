using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.MiscMaster.Command.CreateMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class MiscMasterProfile  : Profile 
    {
        

        public MiscMasterProfile()
        {

             CreateMap<MaintenanceManagement.Domain.Entities.MiscMaster,GetMiscMasterDto>();
             
             CreateMap<MaintenanceManagement.Domain.Entities.MiscMaster,GetMiscMasterAutoCompleteDto>();

             CreateMap<CreateMiscMasterCommand, MaintenanceManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, MaintenanceManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

             CreateMap<DeleteMiscMasterCommand,  MaintenanceManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}