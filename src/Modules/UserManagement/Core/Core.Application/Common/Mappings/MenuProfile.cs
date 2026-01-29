using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Menu.Commands.CreateMenu;
using Core.Application.Menu.Commands.DeleteMenu;
using Core.Application.Menu.Commands.UpdateMenu;
using Core.Application.Menu.Commands.UploadMenu;
using Core.Application.Menu.Queries.GetChildMenuByModule;
using Core.Application.Menu.Queries.GetMenuByModule;
using Core.Application.Menu.Queries.GetParentMenu;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            CreateMap<Core.Domain.Entities.Menu, MenuDTO>();
            CreateMap<Core.Domain.Entities.Menu, ChildMenuDTO>();

            CreateMap<CreateMenuCommand, Core.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMenuCommand, Core.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMenuCommand, Core.Domain.Entities.Menu>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<UploadMenuDto, Core.Domain.Entities.Menu>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            
            CreateMap<Core.Domain.Entities.Menu, ParentMenuDto>();
            
        }
    }
}