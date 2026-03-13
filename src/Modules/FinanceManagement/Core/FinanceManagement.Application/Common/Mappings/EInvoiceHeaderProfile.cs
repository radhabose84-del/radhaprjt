using AutoMapper;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.Common.Mappings
{
    public class EInvoiceHeaderProfile : Profile
    {
        public EInvoiceHeaderProfile()
        {
            CreateMap<CreateEInvoiceHeaderCommand, Domain.Entities.EInvoiceHeader>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateEInvoiceHeaderCommand, Domain.Entities.EInvoiceHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
