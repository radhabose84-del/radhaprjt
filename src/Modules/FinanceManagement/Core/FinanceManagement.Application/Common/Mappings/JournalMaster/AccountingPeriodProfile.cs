using AutoMapper;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings.JournalMaster
{
    public class AccountingPeriodProfile : Profile
    {
        public AccountingPeriodProfile()
        {
            CreateMap<CreateAccountingPeriodCommand, FinanceManagement.Domain.Entities.AccountingPeriod>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.PeriodStatus, opt => opt.Ignore());

            CreateMap<UpdateAccountingPeriodCommand, FinanceManagement.Domain.Entities.AccountingPeriod>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.PeriodStatus, opt => opt.Ignore());
        }
    }
}
