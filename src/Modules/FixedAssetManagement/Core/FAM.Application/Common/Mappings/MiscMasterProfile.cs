using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Application.MiscMaster.Command.DeleteMiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class MiscMasterProfile  : Profile 
    {
        

        public MiscMasterProfile()
        {

             CreateMap<FAM.Domain.Entities.MiscMaster,GetMiscMasterDto>();
             
             CreateMap<FAM.Domain.Entities.MiscMaster,GetMiscMasterAutoCompleteDto>();

             CreateMap<CreateMiscMasterCommand, FAM.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, FAM.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

             CreateMap<DeleteMiscMasterCommand,  FAM.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}