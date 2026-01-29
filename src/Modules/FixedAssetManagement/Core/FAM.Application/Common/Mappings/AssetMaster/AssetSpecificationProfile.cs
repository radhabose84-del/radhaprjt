using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetSpecificationProfile : Profile
    {
        public AssetSpecificationProfile()
        { 

            CreateMap<DeleteAssetSpecificationCommand, AssetSpecifications>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            
            CreateMap<CreateAssetSpecificationCommand, AssetSpecifications>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateSpecificationItem, AssetSpecifications>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<AssetSpecificationJsonDto, AssetSpecificationAutoCompleteDTO>();
            CreateMap<AssetSpecifications, AssetSpecificationDTO>();      
            CreateMap<AssetSpecificationJsonDto, AssetSpecificationDTO>();
        }       
    }
}