using AutoMapper;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Entities.PurchaseReturn;

namespace PurchaseManagement.Application.Common.Mappings.PurchaseReturn;

public class PurchaseReturnProfile : Profile
{
    public PurchaseReturnProfile()
    {
        CreateMap<PurchaseReturnHeader, PurchaseReturnHeaderDto>()
            .ForMember(d => d.IsActive,            o => o.MapFrom(s => s.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active))
            .ForMember(d => d.IsDeleted,           o => o.MapFrom(s => s.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted))
            .ForMember(d => d.ReturnTypeCode,      o => o.MapFrom(s => s.ReturnType != null ? s.ReturnType.Code : null))
            .ForMember(d => d.ReturnTypeDescription, o => o.MapFrom(s => s.ReturnType != null ? s.ReturnType.Description : null))
            .ForMember(d => d.ReturnReasonCode,    o => o.MapFrom(s => s.ReturnReason != null ? s.ReturnReason.Code : null))
            .ForMember(d => d.ReturnReasonDescription, o => o.MapFrom(s => s.ReturnReason != null ? s.ReturnReason.Description : null))
            .ForMember(d => d.ReturnActionCode,    o => o.MapFrom(s => s.ReturnAction != null ? s.ReturnAction.Code : null))
            .ForMember(d => d.StatusCode,          o => o.MapFrom(s => s.MiscStatus != null ? s.MiscStatus.Code : null))
            .ForMember(d => d.ReplacementStatusCode, o => o.MapFrom(s => s.ReplacementStatus != null ? s.ReplacementStatus.Code : null))
            .ForMember(d => d.PoNumber,            o => o.Ignore())
            .ForMember(d => d.GrnNo,               o => o.Ignore())
            .ForMember(d => d.VendorName,          o => o.Ignore())
            .ForMember(d => d.Details,             o => o.MapFrom(s => s.Details));

        CreateMap<PurchaseReturnDetail, PurchaseReturnDetailDto>()
            .ForMember(d => d.IsActive,         o => o.MapFrom(s => s.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active))
            .ForMember(d => d.ItemCode,         o => o.Ignore())
            .ForMember(d => d.ItemName,         o => o.Ignore())
            .ForMember(d => d.UomName,          o => o.Ignore())
            .ForMember(d => d.ReturnReasonName, o => o.MapFrom(s => s.ReturnReason != null ? s.ReturnReason.Code : null));

        CreateMap<PurchaseReturnHeader, PurchaseReturnListItemDto>()
            .ForMember(d => d.IsActive,         o => o.MapFrom(s => s.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active))
            .ForMember(d => d.ReturnTypeCode,   o => o.MapFrom(s => s.ReturnType != null ? s.ReturnType.Code : null))
            .ForMember(d => d.ReturnReasonCode, o => o.MapFrom(s => s.ReturnReason != null ? s.ReturnReason.Code : null))
            .ForMember(d => d.StatusCode,       o => o.MapFrom(s => s.MiscStatus != null ? s.MiscStatus.Code : null))
            .ForMember(d => d.PoNumber,         o => o.Ignore())
            .ForMember(d => d.GrnNo,            o => o.Ignore())
            .ForMember(d => d.VendorName,       o => o.Ignore());
    }
}
