using AutoMapper;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class FreightRfqProfile : Profile
    {
        public FreightRfqProfile()
        {
            // FreightRfqNumber (document sequence) + StatusId ("Quotation Pending") are resolved in the handler/repo.
            // Quotation rows are built from the Transporters list in the handler, so ignore the dest collection here.
            CreateMap<CreateFreightRfqCommand, PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()
                .ForMember(d => d.Quotations, opt => opt.Ignore())
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateFreightRfqCommand, PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
