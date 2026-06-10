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
            // FreightRfqNumber + StatusId (default "Draft") are resolved in the command repository.
            // Identity, audit, workflow/approval, and navigation members are NOT set from the command
            // (set by the repository/handler or only during the approval flow) — ignore them so the
            // mapping configuration is explicit and valid.
            CreateMap<CreateFreightRfqCommand, PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.FreightRfqNumber, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.SelectedQuotationId, opt => opt.Ignore())
                .ForMember(d => d.ComparisonRemarks, opt => opt.Ignore())
                .ForMember(d => d.ApprovedTransporterId, opt => opt.Ignore())
                .ForMember(d => d.ApprovedRate, opt => opt.Ignore())
                .ForMember(d => d.ApprovedFreightValue, opt => opt.Ignore())
                .ForMember(d => d.ApprovalRemarks, opt => opt.Ignore())
                .ForMember(d => d.Quotations, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())
                .ForMember(d => d.CreatedByName, opt => opt.Ignore())
                .ForMember(d => d.CreatedIP, opt => opt.Ignore())
                .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
                .ForMember(d => d.ModifiedDate, opt => opt.Ignore())
                .ForMember(d => d.ModifiedByName, opt => opt.Ignore())
                .ForMember(d => d.ModifiedIP, opt => opt.Ignore());

            CreateMap<UpdateFreightRfqCommand, PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(d => d.IsDeleted, opt => opt.Ignore())
                .ForMember(d => d.FreightRfqNumber, opt => opt.Ignore())
                .ForMember(d => d.RfqDate, opt => opt.Ignore())
                .ForMember(d => d.StatusId, opt => opt.Ignore())
                .ForMember(d => d.SelectedQuotationId, opt => opt.Ignore())
                .ForMember(d => d.ComparisonRemarks, opt => opt.Ignore())
                .ForMember(d => d.ApprovedTransporterId, opt => opt.Ignore())
                .ForMember(d => d.ApprovedRate, opt => opt.Ignore())
                .ForMember(d => d.ApprovedFreightValue, opt => opt.Ignore())
                .ForMember(d => d.ApprovalRemarks, opt => opt.Ignore())
                .ForMember(d => d.Quotations, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())
                .ForMember(d => d.CreatedByName, opt => opt.Ignore())
                .ForMember(d => d.CreatedIP, opt => opt.Ignore())
                .ForMember(d => d.ModifiedBy, opt => opt.Ignore())
                .ForMember(d => d.ModifiedDate, opt => opt.Ignore())
                .ForMember(d => d.ModifiedByName, opt => opt.Ignore())
                .ForMember(d => d.ModifiedIP, opt => opt.Ignore());
        }
    }
}
