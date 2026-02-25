using AutoMapper;
using UserManagement.Application.Menu.Commands.CreateMenu;
using UserManagement.Application.Menu.Commands.DeleteMenu;
using UserManagement.Application.Menu.Commands.UpdateMenu;
using UserManagement.Application.Menu.Commands.UploadMenu;
using UserManagement.Application.Menu.Queries.GetChildMenuByModule;
using UserManagement.Application.Menu.Queries.GetMenuByModule;
using UserManagement.Application.Menu.Queries.GetParentMenu;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            CreateMap<UserManagement.Domain.Entities.Menu, MenuDTO>();
            CreateMap<UserManagement.Domain.Entities.Menu, ChildMenuDTO>();

            CreateMap<CreateMenuCommand, UserManagement.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMenuCommand, UserManagement.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMenuCommand, UserManagement.Domain.Entities.Menu>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<UploadMenuDto, UserManagement.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
            CreateMap<UserManagement.Domain.Entities.Menu, ParentMenuDto>();
            
        }
    }
}