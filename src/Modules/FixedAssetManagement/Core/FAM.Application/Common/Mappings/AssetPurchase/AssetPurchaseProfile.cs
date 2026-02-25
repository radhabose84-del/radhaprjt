using AutoMapper;
using FAM.Application.AssetMaster.AssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGrnDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRNItem;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Application.Common.Mappings.AssetPurchase
{
    public class AssetPurchaseProfile : Profile
    {
        
        public AssetPurchaseProfile()
        {
               CreateMap<FAM.Domain.Entities.AssetSource, AssetSourceAutoCompleteDto>(); 
               CreateMap<AssetUnit, AssetUnitAutoCompleteDto>(); 
               CreateMap<AssetGrn, GetAssetGrnDto>();
               CreateMap<AssetGrnItem, AssetGrnItemDto>();
               CreateMap<AssetGrnDetails, AssetDetailsDto>();
               CreateMap<AssetPurchaseDetails,AssetPurchaseDetailsDto>();
               CreateMap<CreateAssetPurchaseDetailCommand, AssetPurchaseDetails>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CapitalizationDate, opt => opt.MapFrom(src => src.CapitalizationDate ?? null));
               CreateMap<UpdateAssetPurchaseDetailCommand, AssetPurchaseDetails>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.AssetSourceId, opt => opt.Ignore())
                .ForMember(dest => dest.OldUnitId, opt => opt.Ignore())
                .ForMember(dest => dest.CapitalizationDate, opt => opt.MapFrom(src => src.CapitalizationDate ?? null));
            

        }      
    }
}