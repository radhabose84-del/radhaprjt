using AutoMapper;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class PeriodStatusOverrideProfile : Profile
    {
        public PeriodStatusOverrideProfile()
        {
            CreateMap<RequestPeriodReversalCommand, Domain.Entities.PeriodStatusOverride>()
                .ForMember(d => d.CompanyId,          opt => opt.Ignore())   // set server-side
                .ForMember(d => d.RequestedBy,       opt => opt.Ignore())    // set from session
                .ForMember(d => d.RequestedAt,       opt => opt.Ignore())    // set in handler
                .ForMember(d => d.FromStatusId,      opt => opt.Ignore())    // resolved in handler
                .ForMember(d => d.ToStatusId,        opt => opt.Ignore())    // resolved in handler
                .ForMember(d => d.OverrideStatusId,  opt => opt.Ignore())    // set to PENDING id in handler
                .ForMember(d => d.IsActive,          opt => opt.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted,         opt => opt.MapFrom(_ => IsDelete.NotDeleted));
        }
    }
}
