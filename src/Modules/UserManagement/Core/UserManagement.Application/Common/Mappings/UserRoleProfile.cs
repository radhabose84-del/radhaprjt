using AutoMapper;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.UserRole.Commands.CreateRole;
using UserManagement.Application.UserRole.Commands.UpdateRole;
using UserManagement.Application.UserRole.Commands.DeleteRole;
using UserManagement.Application.UserRole.Queries.GetRolesAutocomplete;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class UserRoleProfile : Profile
    {
        public UserRoleProfile()
        {
            // Domain -> DTO Mapping
          //  CreateMap<  UserManagement.Domain.Entities.UserRole , GetPwdRuleDto >();
                CreateMap<UserManagement.Domain.Entities.UserRole, GetUserRoleDto>() ;

            // Command -> Domain Mapping
            CreateMap<CreateRoleCommand, UserManagement.Domain.Entities.UserRole>()
             .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            // Ensures Id is not set during creation

                CreateMap<UserManagement.Domain.Entities.UserRole, UserRoleDto>()             
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));
            
            // CreateMap<UpdateRoleCommand, UserManagement.Domain.Entities.UserRole>()
            // .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Explicitly map Id in update scenarios
            // .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

             CreateMap<UpdateRoleCommand,  UserManagement.Domain.Entities.UserRole>() 
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


          //  CreateMap<DeleteRoleCommand, UserManagement.Domain.Entities.UserRole>();
            CreateMap<DeleteRoleCommand, UserManagement.Domain.Entities.UserRole>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));


            CreateMap<UserManagement.Domain.Entities.UserRole, GetUserRoleAutocompleteDto>();
            //CreateMap<UserManagement.Domain.Entities.UserRole, UserRoleDto>();
            // CreateMap<UserRoleStatusDto, UserManagement.Domain.Entities.UserRole>()
            // .ForMember(dest => dest.Id, opt => opt.Ignore())
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
         
        }

    }
}
