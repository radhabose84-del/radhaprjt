using AutoMapper;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class TaxCodeProfile : Profile
    {
        public TaxCodeProfile()
        {
            // Tax Code Master
            CreateMap<CreateTaxCodeMasterCommand, Domain.Entities.TaxCodeMaster>()
                .ForMember(dest => dest.TaxComponent, opt => opt.MapFrom(src => src.TaxComponent ?? "COMBINED"))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateTaxCodeMasterCommand, Domain.Entities.TaxCodeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Tax Account Linkage
            CreateMap<CreateTaxAccountLinkageCommand, Domain.Entities.TaxAccountLinkage>()
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => "PENDING"))
                .ForMember(dest => dest.IsActivated, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // GSTR Section Mapping
            CreateMap<CreateGstrSectionMappingCommand, Domain.Entities.GstrSectionMapping>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateGstrSectionMappingCommand, Domain.Entities.GstrSectionMapping>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
