#nullable disable
using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FluentValidation;

namespace FAM.Presentation.Validation.AssetMaster.AssetTransferIssue
{
    public class CreateAssetTransferIssueCommandValidator : AbstractValidator<CreateAssetTransferIssueCommand>
    {
        public CreateAssetTransferIssueCommandValidator()
        {
          RuleFor(x => x.AssetTransferIssueHdrDto.DocDate)
                .NotEmpty().WithMessage("Document Date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Document Date cannot be in the future.");           

            RuleFor(x => x.AssetTransferIssueHdrDto.FromUnitId)
                .GreaterThan(0).WithMessage("From Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto.ToUnitId)
                .GreaterThan(0).WithMessage("To Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto.FromDepartmentId)
                .GreaterThan(0).WithMessage("From Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto.ToDepartmentId)
                .GreaterThan(0).WithMessage("To Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto.FromCustodianId)
                .GreaterThan(0).WithMessage("From Custodian ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto.ToCustodianId)
                .GreaterThan(0).WithMessage("To Custodian ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferIssueHdrDto.Status)
            //     .NotEmpty().WithMessage("Status is required.")
            //     .Must(s => s == "Pending" || s == "Approved" || s == "Rejected")
            //     .WithMessage("Status must be 'Pending', 'Approved', or 'Rejected'.");

            RuleFor(x => x.AssetTransferIssueHdrDto.AssetTransferIssueDtls)
                .NotEmpty().WithMessage("At least one asset must be included.")
                .Must(AssetTransferIssueDtls => AssetTransferIssueDtls.All(a => a.AssetId > 0))
                .WithMessage("All assets must have a valid Asset ID.")
                .Must(AssetTransferIssueDtls => AssetTransferIssueDtls.All(a => a.AssetValue > 0))
                .WithMessage("All assets must have a valid Asset Value.");
        }
        
    }
}