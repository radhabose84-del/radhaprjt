using AutoMapper;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.Shared;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;
using static InventoryManagement.Domain.Common.BaseEntity;


namespace InventoryManagement.Application.Common.Mappings.Item
{
    public class ItemCategoryProfile : Profile
    {
        public ItemCategoryProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.Item.ItemCategory, ItemCategoryDto>();
            CreateMap<InventoryManagement.Domain.Entities.Item.ItemCategory, ItemCategoryAutoCompleteDto>();

            CreateMap<CreateItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupId, opt => opt.MapFrom(src => src.ItemGroupId))
                .ForMember(dest => dest.ItemCategoryName, opt => opt.MapFrom(src => src.ItemCategoryName))
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.IsBudgetApplicable, opt => opt.MapFrom(src => src.IsBudgetApplicable))
                .ForMember(dest => dest.EmergencyPoApplicable, opt => opt.MapFrom(src => src.EmergencyPoApplicable))
                .ForMember(dest => dest.EmergencyPoLimit, opt => opt.MapFrom(src => src.EmergencyPoLimit))
                .ForMember(dest => dest.ItemCategoryModules, opt => opt.Ignore())
                .ForMember(dest => dest.UnitConfigs, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.ItemGroupId, opt => opt.MapFrom(src => src.ItemGroupId))
                .ForMember(dest => dest.ItemCategoryName, opt => opt.MapFrom(src => src.ItemCategoryName))
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId))
                .ForMember(dest => dest.IsBudgetApplicable, opt => opt.MapFrom(src => src.IsBudgetApplicable))
                .ForMember(dest => dest.EmergencyPoApplicable, opt => opt.MapFrom(src => src.EmergencyPoApplicable))
                .ForMember(dest => dest.EmergencyPoLimit, opt => opt.MapFrom(src => src.EmergencyPoLimit))
                .ForMember(dest => dest.ItemCategoryModules, opt => opt.Ignore())
                .ForMember(dest => dest.UnitConfigs, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));


            CreateMap<DeleteItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<SampleQuantityItem, InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.ItemCategory, opt => opt.Ignore())
                .ForMember(dest => dest.UOM, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig, SampleQuantityDto>()
                .ForMember(dest => dest.UnitName, opt => opt.Ignore())
                .ForMember(dest => dest.UOMName, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active ? 1 : 0));
        }
    }
}
