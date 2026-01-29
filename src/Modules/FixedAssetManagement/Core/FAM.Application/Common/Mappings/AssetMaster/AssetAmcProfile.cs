using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetExistingVendorDetails;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetAmcProfile : Profile
    {
        public AssetAmcProfile()
        {
            CreateMap<ExistingVendorDetails, GetExistingVendorDetailsDto>();
            CreateMap<AssetAmc, AssetAmcDto>();
            CreateMap<CreateAssetAmcCommand, AssetAmc>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
            CreateMap<UpdateAssetAmcCommand, AssetAmc>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
            CreateMap<DeleteAssetAmcCommand, AssetAmc>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));     
            
        }
    }
}