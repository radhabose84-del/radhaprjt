using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FluentValidation;

namespace FAM.API.Validation.AssetMaster.AssetTransferIssueApproval
{
    public class UpdateAssetTransferIssueApprovalValidator : AbstractValidator<UpdateAssetTranferIssueApprovalCommand>
    {
    public UpdateAssetTransferIssueApprovalValidator()
    {
             RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(x => x == "Approved" || x == "Rejected").WithMessage("Invalid status Status Should be Approved or Rejected.");
    }
    }
}