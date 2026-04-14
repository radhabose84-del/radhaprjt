using AutoMapper;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ProformaInvoiceProfile : Profile
    {
        public ProformaInvoiceProfile()
        {
            CreateMap<CreateProformaInvoiceCommand, Domain.Entities.ProformaInvoice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProformaNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SOBalance, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceivedAmount, opt => opt.MapFrom(src => 0m))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateProformaInvoiceCommand, Domain.Entities.ProformaInvoice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
