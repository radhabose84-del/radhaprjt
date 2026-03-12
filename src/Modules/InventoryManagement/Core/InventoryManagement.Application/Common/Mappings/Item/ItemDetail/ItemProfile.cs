using AutoMapper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using DomainStatus = InventoryManagement.Domain.Common.BaseEntity.Status;

namespace InventoryManagement.Application.Common.Mappings.Item.ItemDetail
{
    public sealed class ItemProfile : Profile
    {
        public ItemProfile()
        {
            // ---------------- WRITE MAPS (DTO -> Entity) ----------------
            CreateMap<byte, DomainStatus>()
                .ConvertUsing(b => b == 1 ? DomainStatus.Active : DomainStatus.Inactive);

            CreateMap<DomainStatus, byte>()
                .ConvertUsing(s => s == DomainStatus.Active ? (byte)1 : (byte)0);

            CreateMap<ItemDto, ItemMaster>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ParentItemId,
                    o => o.MapFrom(src => src.ParentItemId > 0 ? src.ParentItemId : (int?)null))
                .ForMember(d => d.IsActive, o => o.MapFrom(src => src.IsActive))
                // navs ignored – handled elsewhere
                .ForMember(d => d.ChildItems, o => o.Ignore())
                .ForMember(d => d.Purchase, o => o.Ignore())
                .ForMember(d => d.Inventory, o => o.Ignore())
                .ForMember(d => d.Quality, o => o.Ignore())
                .ForMember(d => d.Sale, o => o.Ignore())
                .ForMember(d => d.VariantValues, o => o.Ignore())
                .ForMember(d => d.VariantAttributes, o => o.Ignore())
                .ForMember(d => d.Suppliers, o => o.Ignore())
                .ForMember(d => d.Manufacture, o => o.Ignore())
                .ForMember(d => d.ItemUOMs, o => o.Ignore())
                .ForMember(d => d.ItemUnitMappings, o => o.Ignore())
                .ForMember(d => d.HSNMaster, o => o.Ignore())
                .ForMember(d => d.ItemGroup, o => o.Ignore())
                .ForMember(d => d.ItemCategory, o => o.Ignore())
                .ForMember(d => d.UOM, o => o.Ignore())
                .ForMember(d => d.MiscClassification, o => o.Ignore())
                .ForMember(d => d.MiscStatus, o => o.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ItemPurchaseDto, ItemPurchase>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.PurchaseUOM, o => o.Ignore());

            CreateMap<ItemInventoryDto, ItemInventory>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.WeightUOM, o => o.Ignore())
                .ForMember(d => d.MiscDefaultMaterialRequestType, o => o.Ignore())
                .ForMember(d => d.MiscValuationMethod, o => o.Ignore())
                .ForMember(d => d.MiscRequestType, o => o.Ignore());

            CreateMap<ItemQualityDto, ItemQuality>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.MiscCertificateType, o => o.Ignore());

            CreateMap<ItemSaleDto, ItemSale>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.SalesUOM, o => o.Ignore());

            CreateMap<ItemUomDto, ItemUOM>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.ConversionUOM, o => o.Ignore());

            CreateMap<ItemUnitMappingDto, ItemUnitMapping>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.Item, o => o.Ignore())
                .ForMember(d => d.ProcurementType, o => o.Ignore())
                .ForMember(d => d.ItemGroup, o => o.Ignore());

            // If you ever need to write attributes via AutoMapper:
            CreateMap<VariantAttributeDto, ItemVariantAttribute>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.ItemMaster, o => o.Ignore())
                .ForMember(d => d.MiscVariantBasedOn, o => o.Ignore())
                .ForMember(d => d.MiscAttributeGroup, o => o.Ignore())
                .ForMember(d => d.MiscAttribute, o => o.Ignore())
                .ForMember(d => d.ItemVariantValues, o => o.Ignore());

            // ---------------- READ MAPS (Entity -> DTO) ----------------
            CreateMap<ItemMaster, ItemDto>()
                .ForMember(d => d.VariantValues,     o => o.MapFrom(s => s.VariantValues))
                .ForMember(d => d.VariantAttributes, o => o.MapFrom(s => s.VariantAttributes));

            CreateMap<ItemMaster, ItemListDto>();

            CreateMap<ItemPurchase, ItemPurchaseDto>();
            CreateMap<ItemInventory, ItemInventoryDto>();
            CreateMap<ItemQuality, ItemQualityDto>();
            CreateMap<ItemSale, ItemSaleDto>();
            CreateMap<ItemUOM, ItemUomDto>();
            CreateMap<ItemSupplier, ItemSupplierDto>();
            CreateMap<ItemManufacture, ItemManufactureDto>();
            CreateMap<ItemUnitMapping, ItemUnitMappingDto>();

            // NEW SCHEMA: value has VariantAttributeId; AttributeId is on the nav
            CreateMap<ItemVariantValue, VariantValueDto>()
                .ForMember(d => d.VariantAttributeId, o => o.MapFrom(s => s.VariantAttributeId))                
                .ForMember(d => d.OptionValue,        o => o.MapFrom(s => s.OptionValue))
                .ForMember(d => d.Combo,              o => o.Ignore()); // Combo is client-side only

            // Attributes → DTO
            CreateMap<ItemVariantAttribute, VariantAttributeDto>()
                .ForMember(d => d.Id,              o => o.MapFrom(s => s.Id))
                .ForMember(d => d.AttributeId,     o => o.MapFrom(s => s.AttributeId))
                .ForMember(d => d.VariantBasedOn,  o => o.MapFrom(s => s.VariantBasedOn))
                .ForMember(d => d.AttributeGroupId,o => o.MapFrom(s => s.AttributeGroupId))
                .ForMember(d => d.Order,           o => o.MapFrom(s => s.Order));

            CreateMap<ItemMaster, GetItemAutoCompleteDto>()
                .ForMember(d => d.Id,       o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ItemCode, o => o.MapFrom(s => s.ItemCode))
                .ForMember(d => d.ItemName, o => o.MapFrom(s => s.ItemName));
        }
    }
}
