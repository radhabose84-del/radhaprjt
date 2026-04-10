
using AutoMapper;
using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.Common.Mappings
{
    public class ItemSpecificationValueProfile : Profile
    {
        public ItemSpecificationValueProfile()
        {
            // Entity to DTO
            CreateMap<DomainEntities.ItemSpecificationValue, ItemSpecificationValueDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.SpecificationMasterName, opt => opt.Ignore());

            CreateMap<DomainEntities.ItemSpecificationValue, ItemSpecificationValueLookupDto>();

            // Command to Entity
            CreateMap<CreateItemSpecificationValueCommand, DomainEntities.ItemSpecificationValue>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateItemSpecificationValueCommand, DomainEntities.ItemSpecificationValue>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
