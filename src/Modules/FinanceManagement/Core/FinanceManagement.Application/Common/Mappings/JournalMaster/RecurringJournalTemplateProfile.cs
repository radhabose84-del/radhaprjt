using AutoMapper;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings.JournalMaster
{
    public class RecurringJournalTemplateProfile : Profile
    {
        public RecurringJournalTemplateProfile()
        {
            CreateMap<CreateRecurringJournalTemplateCommand, FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.Lines, opt => opt.Ignore());

            CreateMap<UpdateRecurringJournalTemplateCommand, FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.Lines, opt => opt.Ignore());
        }
    }
}
