using AutoMapper;
using PartyManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PartyManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile
    {
        public MiscMasterProfile()
        {

             CreateMap<PartyManagement.Domain.Entities.MiscMaster,GetMiscMasterDto>();
             
             CreateMap<PartyManagement.Domain.Entities.MiscMaster,GetMiscMasterAutoCompleteDto>();

             CreateMap<CreateMiscMasterCommand, PartyManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, PartyManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

             CreateMap<DeleteMiscMasterCommand,  PartyManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}