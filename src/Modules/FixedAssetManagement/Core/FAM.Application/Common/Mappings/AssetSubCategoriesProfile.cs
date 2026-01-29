using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class AssetSubCategoriesProfile :Profile
    {
        public AssetSubCategoriesProfile()
        {
             CreateMap<FAM.Domain.Entities.AssetSubCategories,AssetSubCategoriesDto>();
             CreateMap<FAM.Domain.Entities.AssetSubCategories, AssetSubCategoriesAutoCompleteDto>();
             CreateMap<CreateAssetSubCategoriesCommand, FAM.Domain.Entities.AssetSubCategories>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AssetCategoriesId, opt => opt.MapFrom(src => src.AssetCategoriesId))
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAssetSubCategoriesCommand, FAM.Domain.Entities.AssetSubCategories>()
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AssetCategoriesId, opt => opt.MapFrom(src => src.AssetCategoriesId))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteAssetSubCategoriesCommand, FAM.Domain.Entities.AssetSubCategories>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));  
        }
    }
}