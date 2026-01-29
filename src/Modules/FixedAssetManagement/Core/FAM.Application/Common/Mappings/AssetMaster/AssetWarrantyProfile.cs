using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetWarrantyProfile : Profile
    {
        public AssetWarrantyProfile()
        { 
            CreateMap<DeleteAssetWarrantyCommand, AssetWarranties>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));            
            
            CreateMap<CreateAssetWarrantyCommand, AssetWarranties>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted)); 

            CreateMap<UpdateAssetWarrantyCommand, AssetWarranties>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));            

            CreateMap<AssetWarrantyDTO, AssetWarrantyAutoCompleteDTO>();
            CreateMap<AssetWarranties, AssetWarrantyDTO>();      
        }       
    }
}