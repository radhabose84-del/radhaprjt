using AutoMapper;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class TaxCodeProfile : Profile
    {
        public TaxCodeProfile()
        {
            // Tax Code Master (no IsDeleted — soft delete removed; "remove" = IsActive Inactive)
            CreateMap<CreateTaxCodeMasterCommand, Domain.Entities.TaxCodeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active));

            CreateMap<UpdateTaxCodeMasterCommand, Domain.Entities.TaxCodeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Tax Account Linkage — StatusId is set in the handler (create = APPROVED).
            CreateMap<CreateTaxAccountLinkageCommand, Domain.Entities.TaxAccountLinkage>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active));
        }
    }
}
