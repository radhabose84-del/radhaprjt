using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Currency.Commands.CreateCurrency;
using Core.Application.Currency.Commands.DeleteCurrency;
using Core.Application.Currency.Commands.UpdateCurrency;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class CurrencyProfile : Profile
    {
        public CurrencyProfile()
        {
            CreateMap<Core.Domain.Entities.Currency, Core.Application.Currency.Queries.GetCurrency.CurrencyDto>();
            CreateMap<Core.Domain.Entities.Currency, Core.Application.Currency.Queries.GetCurrency.CurrencyAutoCompleteDto>();
            CreateMap<CreateCurrencyCommand, Core.Domain.Entities.Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            CreateMap<UpdateCurrencyCommand, Core.Domain.Entities.Currency>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
            CreateMap<DeleteCurrencyCommand, Core.Domain.Entities.Currency>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));   

        }
    }
}