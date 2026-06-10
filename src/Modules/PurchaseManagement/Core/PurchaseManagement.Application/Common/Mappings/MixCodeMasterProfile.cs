using AutoMapper;
using PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class MixCodeMasterProfile : Profile
    {
        public MixCodeMasterProfile()
        {
            // Entity → DTO (IsActive enum maps to its underlying int: Active=1, Inactive=0)
            CreateMap<PurchaseManagement.Domain.Entities.MixCodeMaster, MixCodeMasterDto>();
            CreateMap<PurchaseManagement.Domain.Entities.MixCodeMaster, MixCodeMasterLookupDto>();

            // Create command → Entity (IsActive/IsDeleted set here — never in the handler)
            CreateMap<CreateMixCodeMasterCommand, PurchaseManagement.Domain.Entities.MixCodeMaster>()
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(_ => IsDelete.NotDeleted));

            // Update command → Entity (MixCode is immutable — not mapped)
            CreateMap<UpdateMixCodeMasterCommand, PurchaseManagement.Domain.Entities.MixCodeMaster>()
                .ForMember(d => d.MixCode, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
