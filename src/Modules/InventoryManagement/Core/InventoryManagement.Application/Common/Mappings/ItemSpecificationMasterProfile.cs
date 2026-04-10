
using AutoMapper;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.Common.Mappings
{
    public class ItemSpecificationMasterProfile : Profile
    {
        public ItemSpecificationMasterProfile()
        {
            // Entity to DTO
            CreateMap<DomainEntities.ItemSpecificationMaster, ItemSpecificationMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<DomainEntities.ItemSpecificationMaster, ItemSpecificationMasterLookupDto>();

            // Command to Entity
            CreateMap<CreateItemSpecificationMasterCommand, DomainEntities.ItemSpecificationMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateItemSpecificationMasterCommand, DomainEntities.ItemSpecificationMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
