using AutoMapper;
using Core.Domain.Entities;
using Core.Application.Modules.Queries.GetModules;
using Core.Application.Modules.Commands.UpdateModule;

namespace Core.Application.Common.Mappings
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            CreateMap<Core.Domain.Entities.Modules, ModuleDto>();
            CreateMap<Core.Domain.Entities.Modules, ModuleByIdDto>()
             .ForMember(dest => dest.Menus, opt => opt.MapFrom(src => src.Menus.Select(m => m.MenuName).ToList()));
            CreateMap<Core.Domain.Entities.Modules, ModuleAutoCompleteDTO>();
            CreateMap<UpdateModuleCommand, Core.Domain.Entities.Modules>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleId));
    }
    }
}