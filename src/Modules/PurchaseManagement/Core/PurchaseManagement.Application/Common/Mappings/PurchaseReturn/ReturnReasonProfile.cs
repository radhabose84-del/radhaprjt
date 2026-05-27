using AutoMapper;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseReturn;

public class ReturnReasonProfile : Profile
{
    public ReturnReasonProfile()
    {
        CreateMap<DomainReturnReason, ReturnReasonDto>()
            .ForMember(d => d.IsActive,         o => o.MapFrom(s => s.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active))
            .ForMember(d => d.IsDeleted,        o => o.MapFrom(s => s.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted))
            .ForMember(d => d.ReturnTypeName,   o => o.MapFrom(s => s.ReturnType != null ? s.ReturnType.Description : null));

        CreateMap<DomainReturnReason, ReturnReasonLookupDto>();
    }
}
