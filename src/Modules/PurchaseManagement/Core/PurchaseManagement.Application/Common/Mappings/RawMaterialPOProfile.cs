using AutoMapper;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class RawMaterialPOProfile : Profile
    {
        public RawMaterialPOProfile()
        {
            // Detail lines + computed totals + StatusId + PONumber + UnitId are set in the handler,
            // not mapped here.
            CreateMap<CreateRawMaterialPOCommand, Domain.Entities.RawMaterialPO.RawMaterialPOHeader>()
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(_ => IsDelete.NotDeleted))
                .ForMember(d => d.RawMaterialPODetails, o => o.Ignore());

            CreateMap<UpdateRawMaterialPOCommand, Domain.Entities.RawMaterialPO.RawMaterialPOHeader>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(d => d.RawMaterialPODetails, o => o.Ignore());
        }
    }
}
