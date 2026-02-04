using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class LanguageProfile : Profile
    {
        public LanguageProfile()
        {
            CreateMap<CreateLanguageCommand, UserManagement.Domain.Entities.Language>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UserManagement.Domain.Entities.Language, LanguageDTO>();

            CreateMap<UpdateLanguageCommand, UserManagement.Domain.Entities.Language>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteLanguageCommand, UserManagement.Domain.Entities.Language>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<UserManagement.Domain.Entities.Language, LanguageAutoCompleteDTO>();
        }
    }
}