using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetTransferReceiptProfile : Profile
    {
        public AssetTransferReceiptProfile()
        {
            
            CreateMap<AssetTransferReceiptHdrDto, FAM.Domain.Entities.AssetMaster.AssetTransferReceiptHdr>()
            .ForMember(dest => dest.AssetTransferReceiptDtl, opt => opt.MapFrom(src => src.AssetTransferReceiptDtl));
            // .ForMember(dest => dest.AssetTransferIssueHdr, opt => opt.Ignore()) ;
            CreateMap<AssetTransferReceiptDtlDto, FAM.Domain.Entities.AssetMaster.AssetTransferReceiptDtl>()
            .ForMember(dest => dest.AckStatus, opt => opt.MapFrom(src => src.AckStatus ?? 0))
            .ForMember(dest => dest.AckDate, opt => opt.MapFrom(src => src.AckStatus == 1 ? DateTimeOffset.UtcNow : (DateTimeOffset?)null));
            

             CreateMap<AssetTransferReceiptDtlDto, FAM.Domain.Entities.AssetMaster.AssetLocation>()
            .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
            .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
            .ForMember(dest => dest.SubLocationId, opt => opt.MapFrom(src => src.SubLocationId))
            .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID ?? 0));


            // CreateMap<AssetTransferIssueHdrIdDto, AssetTransferIssueHdr>()
            // .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetTransferId))
            // .ForMember(dest => dest.AckStatus, opt => opt.MapFrom(src => (byte)1));  // Ensure AckStatus is always 1
        }
    }
}