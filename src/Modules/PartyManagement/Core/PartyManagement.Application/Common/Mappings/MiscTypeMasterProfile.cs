using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
                CreateMap<PartyManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();

                CreateMap<PartyManagement.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterAutocompleteDto>();

                CreateMap<CreateMiscTypeMasterCommand, PartyManagement.Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

                CreateMap<UpdateMiscTypeMasterCommand, PartyManagement.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<DeleteMiscTypeMasterCommand,  PartyManagement.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }
    }
}