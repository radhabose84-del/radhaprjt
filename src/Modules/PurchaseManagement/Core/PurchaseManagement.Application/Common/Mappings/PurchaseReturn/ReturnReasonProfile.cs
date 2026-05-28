using AutoMapper;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseReturn;

public class ReturnReasonProfile : Profile
{
    public ReturnReasonProfile()
    {
        CreateMap<DomainReturnReason, ReturnReasonDto>()
            .ForMember(d => d.IsActive,         o => o.MapFrom(s => s.IsActive == Status.Active))
            .ForMember(d => d.IsDeleted,        o => o.MapFrom(s => s.IsDeleted == IsDelete.Deleted))
            .ForMember(d => d.ReturnTypeName,   o => o.MapFrom(s => s.ReturnType != null ? s.ReturnType.Description : null));

        CreateMap<DomainReturnReason, ReturnReasonLookupDto>();

        CreateMap<CreateReturnReasonCommand, DomainReturnReason>()
            .ForMember(d => d.IsActive,  o => o.MapFrom(_ => Status.Active))
            .ForMember(d => d.IsDeleted, o => o.MapFrom(_ => IsDelete.NotDeleted));

        CreateMap<UpdateReturnReasonCommand, DomainReturnReason>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
    }
}
