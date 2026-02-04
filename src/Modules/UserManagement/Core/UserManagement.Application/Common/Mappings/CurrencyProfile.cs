using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class CurrencyProfile : Profile
    {
        public CurrencyProfile()
        {
            CreateMap<UserManagement.Domain.Entities.Currency, UserManagement.Application.Currency.Queries.GetCurrency.CurrencyDto>();
            CreateMap<UserManagement.Domain.Entities.Currency, UserManagement.Application.Currency.Queries.GetCurrency.CurrencyAutoCompleteDto>();
            CreateMap<CreateCurrencyCommand, UserManagement.Domain.Entities.Currency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
            CreateMap<UpdateCurrencyCommand, UserManagement.Domain.Entities.Currency>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
            CreateMap<DeleteCurrencyCommand, UserManagement.Domain.Entities.Currency>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));   

        }
    }
}