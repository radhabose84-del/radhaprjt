using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetAdditionalCostProfile : Profile
    {
        public AssetAdditionalCostProfile()
        {
           CreateMap<AssetAdditionalCost,AssetAdditionalCostDto>();
           CreateMap<CreateAssetAdditionalCostCommand, AssetAdditionalCost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
           CreateMap<UpdateAssetAdditionalCostCommand, AssetAdditionalCost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.AssetSourceId, opt => opt.Ignore());

            

        }
    }
}