using AutoMapper;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.CreateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSection;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSection;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class ScheduleIIIProfile : Profile
    {
        public ScheduleIIIProfile()
        {
            CreateMap<CreateMasterCommand, Domain.Entities.ScheduleIIIMaster>()
                .ForMember(d => d.IsActive,  o => o.MapFrom(s => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => IsDelete.NotDeleted));

            CreateMap<UpdateMasterCommand, Domain.Entities.ScheduleIIIMaster>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<CreateLineItemCommand, Domain.Entities.ScheduleIIISectionItem>()
                .ForMember(d => d.IsActive,  o => o.MapFrom(s => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => IsDelete.NotDeleted));

            CreateMap<UpdateLineItemCommand, Domain.Entities.ScheduleIIISectionItem>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<CreateSectionCommand, Domain.Entities.ScheduleIIISection>()
                .ForMember(d => d.IsActive,  o => o.MapFrom(s => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => IsDelete.NotDeleted));

            CreateMap<UpdateSectionCommand, Domain.Entities.ScheduleIIISection>()
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
