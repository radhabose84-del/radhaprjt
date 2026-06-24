using AutoMapper;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings.JournalMaster
{
    public class JournalProfile : Profile
    {
        public JournalProfile()
        {
            // Header scalars only — computed fields (totals, status, source, period) and lines are set in the handler.
            CreateMap<CreateJournalCommand, FinanceManagement.Domain.Entities.JournalHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.Details, opt => opt.Ignore());

            CreateMap<UpdateJournalCommand, FinanceManagement.Domain.Entities.JournalHeader>()
                .ForMember(dest => dest.Details, opt => opt.Ignore());
        }
    }
}
