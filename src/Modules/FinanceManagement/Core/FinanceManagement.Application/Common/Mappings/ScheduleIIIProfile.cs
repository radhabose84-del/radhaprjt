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
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateLineItemCommand, Domain.Entities.ScheduleIIILineItem>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<CreateSubTotalCommand, Domain.Entities.ScheduleIIISubTotal>()
                .ForMember(dest => dest.IsActive,          opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted,         opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.IsSystemDefined,   opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.FormulaExpression, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.Formulas,          opt => opt.Ignore());
        }
    }
}
