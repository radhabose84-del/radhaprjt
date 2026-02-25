
using AutoMapper;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Mappings;
public sealed class PortMasterMappingProfile : Profile
{
    public PortMasterMappingProfile()
    {
        CreateMap<CreatePortMasterCommand, PortMaster>()
    .ForMember(d => d.Id, o => o.Ignore())
    .ForMember(d => d.CreatedDate, o => o.Ignore())
    .ForMember(d => d.ModifiedBy, o => o.Ignore())
    .ForMember(d => d.ModifiedDate, o => o.Ignore())
    .ForMember(d => d.IsDeleted, o => o.Ignore());
     

CreateMap<UpdatePortMasterCommand, PortMaster>()
    .ForMember(d => d.CreatedBy,    o => o.Ignore())
    .ForMember(d => d.CreatedDate,  o => o.Ignore())
    .ForMember(d => d.IsDeleted,    o => o.Ignore())
     .ForMember(d => d.IsActive,      o => o.MapFrom(s => s.IsActive == 1 ? 1 : 0));

    }
}
