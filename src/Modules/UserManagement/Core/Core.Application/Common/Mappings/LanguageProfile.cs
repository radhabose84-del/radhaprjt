using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Language.Commands.CreateLanguage;
using Core.Application.Language.Commands.DeleteLanguage;
using Core.Application.Language.Commands.UpdateLanguage;
using Core.Application.Language.Queries.GetLanguages;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class LanguageProfile : Profile
    {
        public LanguageProfile()
        {
            CreateMap<CreateLanguageCommand, Core.Domain.Entities.Language>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<Core.Domain.Entities.Language, LanguageDTO>();

            CreateMap<UpdateLanguageCommand, Core.Domain.Entities.Language>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteLanguageCommand, Core.Domain.Entities.Language>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<Core.Domain.Entities.Language, LanguageAutoCompleteDTO>();
        }
    }
}