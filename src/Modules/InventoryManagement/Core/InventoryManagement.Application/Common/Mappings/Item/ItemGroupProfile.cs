using AutoMapper;
using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings.Item
{
    public class ItemGroupProfile : Profile
    {
        public ItemGroupProfile()
        {
            CreateMap<InventoryManagement.Domain.Entities.Item.ItemGroup, ItemGroupDto>();
            CreateMap<InventoryManagement.Domain.Entities.Item.ItemGroup, ItemGroupAutoCompleteDto>();
            CreateMap<CreateItemGroupCommand, InventoryManagement.Domain.Entities.Item.ItemGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ItemGroupCode, opt => opt.MapFrom(src => src.ItemGroupCode))
                .ForMember(dest => dest.ItemGroupName, opt => opt.MapFrom(src => src.ItemGroupName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateItemGroupCommand, InventoryManagement.Domain.Entities.Item.ItemGroup>()
                 .ForMember(dest => dest.ItemGroupCode, opt => opt.MapFrom(src => src.ItemGroupCode))
                .ForMember(dest => dest.ItemGroupName, opt => opt.MapFrom(src => src.ItemGroupName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));


            CreateMap<DeleteItemGroupCommand, InventoryManagement.Domain.Entities.Item.ItemGroup>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

        }
    }
}