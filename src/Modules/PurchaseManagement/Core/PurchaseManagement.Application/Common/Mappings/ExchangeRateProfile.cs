using AutoMapper;
using PurchaseManagement.Application.ExchangeRate.Commands;


namespace PurchaseManagement.Application.Common.Mappings;
public sealed class ExchangeRateMappingProfile : Profile
{
    public ExchangeRateMappingProfile()
    {        
        CreateMap<PurchaseManagement.Domain.Entities.ExchangeRate, ExchangeRateDto>();
    }
}