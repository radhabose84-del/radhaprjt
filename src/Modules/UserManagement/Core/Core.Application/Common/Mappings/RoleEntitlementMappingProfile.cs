using AutoMapper;
using Core.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using Core.Application.RoleEntitlements.Commands.DeleteRoleEntitlement;
using Core.Application.RoleEntitlements.Commands.GetRolePrivileges;
using Core.Application.RoleEntitlements.Commands.UpdateRoleRntitlement;
using Core.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using Core.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using Core.Application.UserRole.Commands.CreateRole;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class RoleEntitlementMappingProfile : Profile
    {
    public RoleEntitlementMappingProfile()
    {
        
        CreateMap<RoleModuleDTO, RoleModule>()
        .ForMember(dest => dest.RoleId, opt => opt.Ignore());
        CreateMap<RoleParentDTO, RoleParent>()
        .ForMember(dest => dest.RoleId, opt => opt.Ignore())
        .ForMember(dest => dest.MenuId,opt => opt.MapFrom(src => src.ParentId));
        CreateMap<RoleChildDTO, RoleChild>()
        .ForMember(dest => dest.RoleId, opt => opt.Ignore())
        .ForMember(dest => dest.MenuId,opt => opt.MapFrom(src => src.ChildId));
        CreateMap<RoleMenuPrivilegesDTO, RoleMenuPrivileges>()
        .ForMember(dest => dest.RoleId, opt => opt.Ignore())
        .ForMember(dest => dest.MenuId,opt => opt.MapFrom(src => src.MenuId));

           CreateMap<RoleModule, RoleModuleDTO>()
            .ForMember(dest => dest.ModuleId, opt => opt.MapFrom(src => src.ModuleId));

        CreateMap<RoleParent, RoleParentDTO>()
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.MenuId));

        CreateMap<RoleChild, RoleChildDTO>()
            .ForMember(dest => dest.ChildId, opt => opt.MapFrom(src => src.MenuId));

        CreateMap<RoleMenuPrivileges, RoleMenuPrivilegesDTO>()
            .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.MenuId));

   CreateMap<(int RoleId, IList<RoleModule> RoleModules, IList<RoleParent> RoleParents, 
                   IList<RoleChild> RoleChildren, IList<RoleMenuPrivileges> RoleMenuPrivileges), GetByIdRoleEntitlementDTO>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleModules, opt => opt.MapFrom(src => src.RoleModules))
            .ForMember(dest => dest.RoleParents, opt => opt.MapFrom(src => src.RoleParents))
            .ForMember(dest => dest.RoleChildren, opt => opt.MapFrom(src => src.RoleChildren))
            .ForMember(dest => dest.RoleMenuPrivileges, opt => opt.MapFrom(src => src.RoleMenuPrivileges));

              CreateMap<Core.Domain.Entities.Modules, ModuleDTO>()
            .ForMember(dest => dest.Menus, opt => opt.MapFrom(src => src.Menus));

            CreateMap<Core.Domain.Entities.Menu, RoleMenuDTO>()
            .ForMember(dest => dest.ChildMenus, opt => opt.MapFrom(src => src.ChildMenus))
            .ForMember(dest => dest.MenuPrivileages, opt => opt.MapFrom(src => src.RoleMenus));

            CreateMap<Core.Domain.Entities.RoleMenuPrivileges, MenuPrivileageDTO>();
        
        // CreateMap<Core.Domain.Entities.UserRole,RoleDto>();
        // CreateMap<RoleModule,GetByIdModuleDTO>()
        //  .ForMember(dest => dest.ModuleId, opt => opt.MapFrom(src => src.ModuleId));
        //  CreateMap<Domain.Entities.Menu,MenuDTO>()
        //     .ForMember(dest => dest.ChildMenu, opt => opt.MapFrom(src => src.ChildMenus));

            // CreateMap<Core.Domain.Entities.UserRole,RoleDto>()
            // .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Id));

            // CreateMap<Core.Domain.Entities.RoleMenu,GetByIdPermissionDTO>();
          
    
               
    }
    }
}