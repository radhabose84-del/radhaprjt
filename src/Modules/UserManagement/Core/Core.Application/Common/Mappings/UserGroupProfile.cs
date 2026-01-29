using AutoMapper;
using Core.Application.UserGroup.Commands.CreateUserGroup;
using Core.Application.UserGroup.Commands.DeleteUserGroup;
using Core.Application.UserGroup.Commands.UpdateUesrGroup;
using Core.Application.UserGroup.Queries.GetUserGroup;
using Core.Application.UserGroup.Queries.GetUserGroupAutoComplete;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings 
{
    public class UserGroupProfile  : Profile
    {
        public UserGroupProfile()
        {        
             CreateMap<DeleteUserGroupCommand, Core.Domain.Entities.UserGroup>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
                  
             CreateMap<CreateUserGroupCommand,  Core.Domain.Entities.UserGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateUserGroupCommand,  Core.Domain.Entities.UserGroup>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));    
            CreateMap<Core.Domain.Entities.UserGroup, UserGroupAutoCompleteDto>();
            CreateMap<Core.Domain.Entities.UserGroup, UserGroupDto>();
        }
    }
}