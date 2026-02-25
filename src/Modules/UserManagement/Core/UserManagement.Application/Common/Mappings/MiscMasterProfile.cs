using AutoMapper;
using UserManagement.Application.MiscMaster.Command.CreateMiscMaster;
using UserManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using UserManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class MiscMasterProfile  : Profile 
    {       

        public MiscMasterProfile()
        {

             CreateMap<UserManagement.Domain.Entities.MiscMaster,GetMiscMasterDto>();
             
             CreateMap<UserManagement.Domain.Entities.MiscMaster,GetMiscMasterAutoCompleteDto>();

             CreateMap<CreateMiscMasterCommand, UserManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, UserManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

             CreateMap<DeleteMiscMasterCommand,  UserManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}