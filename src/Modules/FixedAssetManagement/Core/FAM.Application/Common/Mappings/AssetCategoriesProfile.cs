using AutoMapper;
using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class AssetCategoriesProfile : Profile
    {
        public AssetCategoriesProfile()
        {
             CreateMap<FAM.Domain.Entities.AssetCategories,AssetCategoriesDto>();
             CreateMap<FAM.Domain.Entities.AssetCategories, AssetCategoriesAutoCompleteDto>(); 
             CreateMap<CreateAssetCategoriesCommand, FAM.Domain.Entities.AssetCategories>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AssetGroupId, opt => opt.MapFrom(src => src.AssetGroupId))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAssetCategoriesCommand, FAM.Domain.Entities.AssetCategories>()
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AssetGroupId, opt => opt.MapFrom(src => src.AssetGroupId))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteAssetCategoriesCommand, FAM.Domain.Entities.AssetCategories>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }
    }
}