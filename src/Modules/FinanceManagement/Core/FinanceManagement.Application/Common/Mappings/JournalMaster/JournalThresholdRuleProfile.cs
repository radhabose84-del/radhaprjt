using AutoMapper;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings.JournalMaster
{
    public class JournalThresholdRuleProfile : Profile
    {
        public JournalThresholdRuleProfile()
        {
            CreateMap<CreateJournalThresholdRuleCommand, FinanceManagement.Domain.Entities.JournalThresholdRule>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.RuleType, opt => opt.Ignore());

            CreateMap<UpdateJournalThresholdRuleCommand, FinanceManagement.Domain.Entities.JournalThresholdRule>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.RuleType, opt => opt.Ignore());
        }
    }
}
