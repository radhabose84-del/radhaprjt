using AutoMapper;
using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class CurrencyForexConfigProfile : Profile
    {
        public CurrencyForexConfigProfile()
        {
            CreateMap<CreateCurrencyForexConfigCommand, Domain.Entities.CurrencyForexConfig>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())     // set server-side from session
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateCurrencyForexConfigCommand, Domain.Entities.CurrencyForexConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
