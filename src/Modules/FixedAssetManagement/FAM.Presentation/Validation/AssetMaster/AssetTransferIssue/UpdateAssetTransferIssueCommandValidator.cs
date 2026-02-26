using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FluentValidation;

namespace FAM.Presentation.Validation.AssetMaster.AssetTransferIssue
{
    public class UpdateAssetTransferIssueCommandValidator : AbstractValidator<UpdateAssetTransferIssueCommand>
    {
        public UpdateAssetTransferIssueCommandValidator()
        {
            RuleFor(x => x.AssetTransferHdr!.Id).NotEmpty().WithMessage("Id is required.");

           RuleFor(x => x.AssetTransferHdr!.DocDate)
                .NotEmpty().WithMessage("Document Date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Document Date cannot be in the future.");

            RuleFor(x => x.AssetTransferHdr!.TransferType)
                 .NotEmpty().WithMessage("Status is required.");

            RuleFor(x => x.AssetTransferHdr!.FromUnitId)
                .GreaterThan(0).WithMessage("From Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.ToUnitId)
                .GreaterThan(0).WithMessage("To Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.FromDepartmentId)
                .GreaterThan(0).WithMessage("From Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.ToDepartmentId)
                .GreaterThan(0).WithMessage("To Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.FromCustodianId)
                .GreaterThan(0).WithMessage("From Custodian ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.ToCustodianId)
                .GreaterThan(0).WithMessage("To Custodian ID must be greater than 0.");

            RuleFor(x => x.AssetTransferHdr!.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => s == "Pending" || s == "Approved" || s == "Rejected")
                .WithMessage("Status must be 'Pending', 'Approved', or 'Rejected'.");


                    // ✅ Ensure at least one AssetTransferIssueDtl is present
            RuleFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)
                .NotEmpty().WithMessage("At least one asset must be included.")
                .Must(details => details!.Count > 0).WithMessage("At least one asset must be included.");

            RuleFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)

                .NotEmpty().WithMessage("At least one asset must be included.")
                .Must(AssetTransferDetails => AssetTransferDetails!.All(a => a.AssetId > 0))
                    .WithMessage("All assets must have a valid Asset ID.")
                    .Must(AssetTransferDetails => AssetTransferDetails!.All(a => a.AssetValue > 0))
                    .WithMessage("All assets must have a valid Asset Value.");

            RuleFor(x => x)
                .Must(x => x.AssetTransferHdr!.AssetTransferIssueDtl!.All(dtl => dtl.AssetTransferId == x.AssetTransferHdr!.Id))
                .WithMessage("All AssetTransferDtl.AssetTransferId must match AssetTransferHdr.Id.");
        }
    }
}