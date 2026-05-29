using AutoMapper;
using PurchaseManagement.Application.BlanketMaster.Commands.Create;
using PurchaseManagement.Application.BlanketMaster.Commands.Update;
using PurchaseManagement.Domain.Entities.BlanketMaster;

namespace PurchaseManagement.Application.Common.Mappings;

public sealed class BlanketMasterProfile : Profile
{
    public BlanketMasterProfile()
    {
        // Header mappings
        CreateMap<CreateBlanketMasterCommand, BlanketHeader>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UnitId, o => o.Ignore())
            .ForMember(d => d.BlanketNumber, o => o.Ignore())
            .ForMember(d => d.TotalEstimatedValue, o => o.Ignore())
            .ForMember(d => d.Details, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.MiscStatus, o => o.Ignore())
            .ForMember(d => d.MiscProcurementType, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        CreateMap<UpdateBlanketMasterCommand, BlanketHeader>()
            .ForMember(d => d.BlanketNumber, o => o.Ignore())
            .ForMember(d => d.UnitId, o => o.Ignore())
            .ForMember(d => d.BlanketDate, o => o.Ignore())
            .ForMember(d => d.TotalEstimatedValue, o => o.Ignore())
            .ForMember(d => d.Details, o => o.Ignore())
            .ForMember(d => d.MiscStatus, o => o.Ignore())
            .ForMember(d => d.MiscProcurementType, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? 1 : 0));
    }
}
