using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.CompanySettings.Commands.CreateCompanySettings;
using Core.Application.CompanySettings.Commands.UpdateCompanySettings;
using Core.Application.CompanySettings.Queries.GetCompanySettings;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class CompanySettingsProfile : Profile
    {
        public CompanySettingsProfile()
        {
            CreateMap<CreateCompanySettingsCommand, Core.Domain.Entities.CompanySettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Currency, opt =>  opt.Ignore())
            .ForMember(dest => dest.Language, opt =>  opt.Ignore())
            .ForMember(dest => dest.FinancialYear, opt =>  opt.Ignore())
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.FinancialYearId, opt => opt.MapFrom(src => src.FinancialYear))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active));

            CreateMap<UpdateCompanySettingsCommand, Core.Domain.Entities.CompanySettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Currency, opt =>  opt.Ignore())
            .ForMember(dest => dest.Language, opt =>  opt.Ignore())
            .ForMember(dest => dest.FinancialYear, opt =>  opt.Ignore())
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.LanguageId, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.FinancialYearId, opt => opt.MapFrom(src => src.FinancialYear))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<Core.Domain.Entities.CompanySettings, CompanySettingsDTO>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.CurrencyId))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.LanguageId))
            .ForMember(dest => dest.FinancialYear, opt => opt.MapFrom(src => src.FinancialYearId));
        }
    }
}