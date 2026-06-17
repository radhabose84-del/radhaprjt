using AutoMapper;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class GstrSectionProfile : Profile
    {
        public GstrSectionProfile()
        {
            // Section Master (no IsDeleted — "remove" = delete row / IsActive Inactive)
            CreateMap<CreateGstrSectionMasterCommand, Domain.Entities.GstrSectionMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active));

            CreateMap<UpdateGstrSectionMasterCommand, Domain.Entities.GstrSectionMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Section Account Linkage
            CreateMap<CreateGstrSectionAccountLinkageCommand, Domain.Entities.GstrSectionAccountLinkage>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active));

            CreateMap<UpdateGstrSectionAccountLinkageCommand, Domain.Entities.GstrSectionAccountLinkage>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
