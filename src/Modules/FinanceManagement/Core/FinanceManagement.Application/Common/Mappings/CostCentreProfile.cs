using AutoMapper;
using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class CostCentreProfile : Profile
    {
        public CostCentreProfile()
        {
            CreateMap<CreateCostCentreCommand, Domain.Entities.CostCentre>()
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())        // set server-side from JWT
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())     // set server-side from JWT
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateCostCentreCommand, Domain.Entities.CostCentre>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
