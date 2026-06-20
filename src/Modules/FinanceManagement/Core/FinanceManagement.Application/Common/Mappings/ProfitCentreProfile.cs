using AutoMapper;
using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class ProfitCentreProfile : Profile
    {
        public ProfitCentreProfile()
        {
            CreateMap<CreateProfitCentreCommand, Domain.Entities.ProfitCentre>()
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())     // set server-side from JWT
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateProfitCentreCommand, Domain.Entities.ProfitCentre>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
