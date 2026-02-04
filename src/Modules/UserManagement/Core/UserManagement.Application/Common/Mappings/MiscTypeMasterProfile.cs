using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile  : Profile
    {
        public MiscTypeMasterProfile()
        {
                CreateMap<UserManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();

                CreateMap<UserManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterAutocompleteDto>();

                CreateMap<CreateMiscTypeMasterCommand, UserManagement.Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

                CreateMap<UpdateMiscTypeMasterCommand, UserManagement.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<DeleteMiscTypeMasterCommand,  UserManagement.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}