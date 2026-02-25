using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval
{
    public class UpdateAssetTranferIssueApprovalCommand :IRequest<int>
    {
        public List<int>? Id { get; set; }
        public string Status { get; set; }

        public UpdateAssetTranferIssueApprovalCommand(List<int> id, string status)
        {
            Id = id;  // Corrected
            Status = status;
        }
    }
}