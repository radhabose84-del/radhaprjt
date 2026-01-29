using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using Core.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using Core.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class MiscTypeMasterProfile  : Profile
    {
        public MiscTypeMasterProfile()
        {
                CreateMap<Core.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();

                CreateMap<Core.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterAutocompleteDto>();

                CreateMap<CreateMiscTypeMasterCommand, Core.Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

                CreateMap<UpdateMiscTypeMasterCommand, Core.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<DeleteMiscTypeMasterCommand,  Core.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}