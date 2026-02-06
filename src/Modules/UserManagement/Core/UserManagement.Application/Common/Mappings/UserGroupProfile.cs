using AutoMapper;
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.Application.UserGroup.Commands.DeleteUserGroup;
using UserManagement.Application.UserGroup.Commands.UpdateUesrGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings 
{
    public class UserGroupProfile  : Profile
    {
        public UserGroupProfile()
        {        
             CreateMap<DeleteUserGroupCommand, UserManagement.Domain.Entities.UserGroup>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
                  
             CreateMap<CreateUserGroupCommand,  UserManagement.Domain.Entities.UserGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateUserGroupCommand,  UserManagement.Domain.Entities.UserGroup>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));    
            CreateMap<UserManagement.Domain.Entities.UserGroup, UserGroupAutoCompleteDto>();
            CreateMap<UserManagement.Domain.Entities.UserGroup, UserGroupDto>();
        }
    }
}