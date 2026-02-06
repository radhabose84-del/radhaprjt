using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.Common.Mappings
{
    public class PartyGroupProfile : Profile
    {
        public PartyGroupProfile()
        {
            CreateMap<CreatePartyGroupCommand, PartyManagement.Domain.Entities.PartyGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup == 1 ? true : false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdatePartyGroupCommand, PartyManagement.Domain.Entities.PartyGroup>()
                 .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup == 1 ? true : false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeletePartyGroupCommand, PartyManagement.Domain.Entities.PartyGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 

            CreateMap<PartyManagement.Domain.Entities.PartyGroup, PartyGroupDto>();
            CreateMap<PartyManagement.Domain.Entities.PartyGroup, PartyGroupByIdDto>();

            CreateMap<PartyManagement.Domain.Entities.PartyGroup, PartyGroupAutoCompleteDto>();
        }
    }
}