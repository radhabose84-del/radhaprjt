using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings
{
    public class PriceGroupMasterProfile : Profile
    {
        public PriceGroupMasterProfile()
        {
            // Entity → DTO
            CreateMap<InventoryManagement.Domain.Entities.PriceGroupMaster, PriceGroupMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            // Entity → LookupDto (used by lookup repository)
            CreateMap<InventoryManagement.Domain.Entities.PriceGroupMaster, PriceGroupMasterLookupDto>();

            // Create command → Entity (IsActive/IsDeleted defaults via ForMember per CLAUDE.md Rule #15)
            CreateMap<CreatePriceGroupMasterCommand, InventoryManagement.Domain.Entities.PriceGroupMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update command → Entity (IsActive mapped from int 0/1)
            CreateMap<UpdatePriceGroupMasterCommand, InventoryManagement.Domain.Entities.PriceGroupMaster>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
