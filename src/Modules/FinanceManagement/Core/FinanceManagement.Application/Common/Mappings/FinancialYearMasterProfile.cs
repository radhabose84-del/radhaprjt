using AutoMapper;
using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class FinancialYearMasterProfile : Profile
    {
        public FinancialYearMasterProfile()
        {
            CreateMap<CreateFinancialYearMasterCommand, Domain.Entities.FinancialYearMaster>()
                .ForMember(dest => dest.CompanyId,  opt => opt.Ignore())       // set server-side
                .ForMember(dest => dest.StatusId,   opt => opt.Ignore())       // set by handler (resolves 'FYS' OPEN)
                .ForMember(dest => dest.IsActive,   opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted,  opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateFinancialYearMasterCommand, Domain.Entities.FinancialYearMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
