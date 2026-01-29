using AutoMapper;
using Core.Application.UserRole.Queries.GetRole;
using Core.Domain.Entities;
using Core.Application.UserRole.Commands.CreateRole;
using Core.Application.UserRole.Commands.UpdateRole;
using Core.Application.UserRole.Commands.DeleteRole;
using Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using Core.Application.UserRole.Queries.GetRolesAutocomplete;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class UserRoleProfile : Profile
    {
        public UserRoleProfile()
        {
            // Domain -> DTO Mapping
          //  CreateMap<  Core.Domain.Entities.UserRole , GetPwdRuleDto >();
                CreateMap<Core.Domain.Entities.UserRole, GetUserRoleDto>() ;

            // Command -> Domain Mapping
            CreateMap<CreateRoleCommand, Core.Domain.Entities.UserRole>()
             .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))        
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            // Ensures Id is not set during creation

                CreateMap<Core.Domain.Entities.UserRole, UserRoleDto>()             
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));
            
            // CreateMap<UpdateRoleCommand, Core.Domain.Entities.UserRole>()
            // .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // Explicitly map Id in update scenarios
            // .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName))
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

             CreateMap<UpdateRoleCommand,  Core.Domain.Entities.UserRole>() 
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


          //  CreateMap<DeleteRoleCommand, Core.Domain.Entities.UserRole>();
            CreateMap<DeleteRoleCommand, Core.Domain.Entities.UserRole>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));


            CreateMap<Core.Domain.Entities.UserRole, GetUserRoleAutocompleteDto>();
            //CreateMap<Core.Domain.Entities.UserRole, UserRoleDto>();
            // CreateMap<UserRoleStatusDto, Core.Domain.Entities.UserRole>()
            // .ForMember(dest => dest.Id, opt => opt.Ignore())
            // .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
         
        }

    }
}
