using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.MiscMaster.Command.CreateMiscMaster;
using Core.Application.MiscMaster.Command.DeleteMiscMaster;
using Core.Application.MiscMaster.Command.UpdateMiscMaster;
using Core.Application.MiscMaster.Queries.GetMiscMaster;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class MiscMasterProfile  : Profile 
    {       

        public MiscMasterProfile()
        {

             CreateMap<Core.Domain.Entities.MiscMaster,GetMiscMasterDto>();
             
             CreateMap<Core.Domain.Entities.MiscMaster,GetMiscMasterAutoCompleteDto>();

             CreateMap<CreateMiscMasterCommand, Core.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, Core.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

             CreateMap<DeleteMiscMasterCommand,  Core.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}