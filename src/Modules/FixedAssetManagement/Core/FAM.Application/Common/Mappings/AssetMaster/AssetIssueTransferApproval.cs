using AutoMapper;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetIssueTransferApproval : Profile
    {
        public AssetIssueTransferApproval()
        {
           CreateMap<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval, AssetTransferIssueApprovalDto>();
           CreateMap<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval, AssetTransferIssueByIdDto>();
           CreateMap<UpdateAssetTranferIssueApprovalCommand, FAM.Domain.Entities.AssetMaster.AssetTransferIssueHdr>()
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
           .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore Id to prevent mapping issues 
      
        }
    }
}