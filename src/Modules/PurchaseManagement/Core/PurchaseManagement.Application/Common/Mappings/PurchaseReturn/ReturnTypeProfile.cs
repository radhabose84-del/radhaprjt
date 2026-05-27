using AutoMapper;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseReturn;

public class ReturnTypeProfile : Profile
{
    public ReturnTypeProfile()
    {
        CreateMap<DomainReturnType, ReturnTypeDto>()
            .ForMember(d => d.IsActive,  o => o.MapFrom(s => s.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active))
            .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted))
            .ForMember(d => d.InventoryImpactName, o => o.MapFrom(s => s.InventoryImpact != null ? s.InventoryImpact.Code : null))
            .ForMember(d => d.FinanceImpactName,   o => o.MapFrom(s => s.FinanceImpact != null ? s.FinanceImpact.Code : null));

        CreateMap<DomainReturnType, ReturnTypeLookupDto>();
    }
}
