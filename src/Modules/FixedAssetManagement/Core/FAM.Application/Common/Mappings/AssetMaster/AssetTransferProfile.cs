using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetTransferProfile : Profile
    
    {
        public AssetTransferProfile()
        {

            CreateMap<FAM.Domain.Entities.AssetMaster.AssetTransferIssue, AssetTransferDto>();

            CreateMap<AssetTransferIssueHdrDto, FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>()
             .ForMember(dest => dest.AssetTransferIssueDtl, opt => opt.MapFrom(src => src.AssetTransferIssueDtls))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));
            CreateMap<AssetTransferIssueDtlDto, FAM.Domain.Entities.AssetMaster.AssetTransferIssueDtl>();




            // ✅ Ensure mapping from AssetTransferJsonDto -> AssetTransferIssueHdr
            CreateMap<AssetTransferJsonDto, FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());// Ignore ID if not required
                                                                 // .ForMember(dest => dest.SomeOtherField, opt => opt.MapFrom(src => src.SomeSourceField)); // Customize field mappings as needed

            // ✅ Ensure mapping from UpdateAssetTransferIssueCommand -> AssetTransferIssueHdr
            CreateMap<UpdateAssetTransferHdrDto, FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>()
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore()) // Ignore modified date if set manually
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.AssetTransferIssueDtl, opt => opt.MapFrom(src => src.AssetTransferIssueDtl));

            CreateMap<UpdateAssetTransferDtlDto, FAM.Domain.Entities.AssetMaster.AssetTransferIssueDtl>();

            CreateMap<AssetMasterDto, GetAssetMasterDto>();

            CreateMap<GetAssetMasterDto, GetCategoryByDeptIdDto>(); 

          CreateMap<AssetTransferIssueHdrDto, AssetTransferApprovalRequestDto>();

            
            

          


          

        }
        
    }
}