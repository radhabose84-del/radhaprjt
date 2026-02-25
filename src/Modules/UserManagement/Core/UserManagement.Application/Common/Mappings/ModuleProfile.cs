#nullable disable
using AutoMapper;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Application.Modules.Commands.UpdateModule;

namespace UserManagement.Application.Common.Mappings
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            CreateMap<UserManagement.Domain.Entities.Modules, ModuleDto>();
            CreateMap<UserManagement.Domain.Entities.Modules, ModuleByIdDto>()
             .ForMember(dest => dest.Menus, opt => opt.MapFrom(src => src.Menus.Select(m => m.MenuName).ToList()));
            CreateMap<UserManagement.Domain.Entities.Modules, ModuleAutoCompleteDTO>();
            CreateMap<UpdateModuleCommand, UserManagement.Domain.Entities.Modules>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleId));
    }
    }
}