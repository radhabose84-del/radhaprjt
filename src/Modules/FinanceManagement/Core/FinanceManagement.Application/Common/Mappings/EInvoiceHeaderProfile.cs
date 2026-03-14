using AutoMapper;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class EInvoiceHeaderProfile : Profile
    {
        public EInvoiceHeaderProfile()
        {
            CreateMap<CreateEInvoiceDetailDto, Domain.Entities.EInvoiceDetail>();

            CreateMap<CreateEInvoiceHeaderCommand, Domain.Entities.EInvoiceHeader>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.EInvoiceDetails, opt => opt.MapFrom(src => src.Details));

            CreateMap<UpdateEInvoiceHeaderCommand, Domain.Entities.EInvoiceHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
