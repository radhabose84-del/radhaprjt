using AutoMapper;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseReturn;

public class ReturnTypeProfile : Profile
{
    public ReturnTypeProfile()
    {
        CreateMap<DomainReturnType, ReturnTypeDto>()
            .ForMember(d => d.IsActive,  o => o.MapFrom(s => s.IsActive == Status.Active))
            .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted == IsDelete.Deleted))
            .ForMember(d => d.InventoryImpactName, o => o.MapFrom(s => s.InventoryImpact != null ? s.InventoryImpact.Code : null))
            .ForMember(d => d.FinanceImpactName,   o => o.MapFrom(s => s.FinanceImpact != null ? s.FinanceImpact.Code : null));

        CreateMap<DomainReturnType, ReturnTypeLookupDto>();

        CreateMap<CreateReturnTypeCommand, DomainReturnType>()
            .ForMember(d => d.IsActive,  o => o.MapFrom(_ => Status.Active))
            .ForMember(d => d.IsDeleted, o => o.MapFrom(_ => IsDelete.NotDeleted));

        CreateMap<UpdateReturnTypeCommand, DomainReturnType>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
    }
}
