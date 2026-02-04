using AutoMapper;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using static InventoryManagement.Domain.Common.BaseEntity;


namespace InventoryManagement.Application.Common.Mappings.Item
{
    public class ItemCategoryProfile : Profile
    {
        public ItemCategoryProfile()
        {
           CreateMap<InventoryManagement.Domain.Entities.Item.ItemCategory,ItemCategoryDto>();
           CreateMap<InventoryManagement.Domain.Entities.Item.ItemCategory, ItemCategoryAutoCompleteDto>();
            CreateMap<CreateItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupId, opt => opt.MapFrom(src => src.ItemGroupId))
                .ForMember(dest => dest.ItemCategoryName, opt => opt.MapFrom(src => src.ItemCategoryName))
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId)) 
                .ForMember(dest => dest.IsBudgetApplicable, opt => opt.MapFrom(src => src.IsBudgetApplicable)) 
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.ItemGroupId, opt => opt.MapFrom(src => src.ItemGroupId))
                .ForMember(dest => dest.ItemCategoryName, opt => opt.MapFrom(src => src.ItemCategoryName))
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId)) 
                .ForMember(dest => dest.IsBudgetApplicable, opt => opt.MapFrom(src => src.IsBudgetApplicable))    
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


              CreateMap<DeleteItemCategoryCommand, InventoryManagement.Domain.Entities.Item.ItemCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    
        }
    }
}