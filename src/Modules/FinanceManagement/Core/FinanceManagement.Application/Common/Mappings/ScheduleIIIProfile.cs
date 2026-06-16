using AutoMapper;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class ScheduleIIIProfile : Profile
    {
        public ScheduleIIIProfile()
        {
            CreateMap<CreateLineItemCommand, Domain.Entities.ScheduleIIILineItem>()
                .ForMember(d => d.IsActive,  o => o.MapFrom(s => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => IsDelete.NotDeleted));

            CreateMap<UpdateLineItemCommand, Domain.Entities.ScheduleIIILineItem>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<CreateSubTotalCommand, Domain.Entities.ScheduleIIISubTotal>()
                .ForMember(d => d.IsActive,          o => o.MapFrom(s => Status.Active))
                .ForMember(d => d.IsDeleted,         o => o.MapFrom(s => IsDelete.NotDeleted))
                .ForMember(d => d.IsSystemDefined,   o => o.MapFrom(s => false))
                .ForMember(d => d.FormulaExpression, o => o.MapFrom(s => string.Empty))
                .ForMember(d => d.Formulas,          o => o.Ignore());
        }
    }
}
