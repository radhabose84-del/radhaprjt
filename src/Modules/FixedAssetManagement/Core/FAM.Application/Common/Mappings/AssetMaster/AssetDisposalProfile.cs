using AutoMapper;
using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetDisposalProfile : Profile
    {
        public AssetDisposalProfile()
        {
           CreateMap<AssetDisposal,AssetDisposalDto>();
           CreateMap<CreateAssetDisposalCommand, AssetDisposal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
           CreateMap<UpdateAssetDisposalCommand, AssetDisposal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.AssetPurchaseId, opt => opt.Ignore());
        }   
    }
}