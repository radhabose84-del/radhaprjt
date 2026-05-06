using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;

namespace FAM.Presentation.Validation.AssetMaster.AssetTransferIssue
{
    public class UpdateAssetTransferIssueCommandValidator : AbstractValidator<UpdateAssetTransferIssueCommand>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;

        public UpdateAssetTransferIssueCommandValidator(IAssetTransferQueryRepository assetTransferQueryRepository)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;

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

            // SCRUM-1463: within-payload duplicate — same AssetId listed twice in the same submission
            RuleFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)
                .Must(dtls => dtls!.Select(d => d.AssetId).Distinct().Count() == dtls!.Count)
                .WithMessage("The same asset cannot appear more than once in the transfer.")
                .When(x => x.AssetTransferHdr?.AssetTransferIssueDtl is { Count: > 0 });

            // SCRUM-1463: cross-payload duplicate — block if any listed asset has a different
            // Pending or Approved-but-not-acknowledged transfer. excludeTransferId = Hdr.Id so a
            // self-update doesn't trip against its own row.
            RuleFor(x => x.AssetTransferHdr!.AssetTransferIssueDtl)
                .MustAsync(async (cmd, dtls, ct) =>
                {
                    var excludeId = cmd.AssetTransferHdr!.Id;
                    foreach (var d in dtls!)
                    {
                        if (await _assetTransferQueryRepository.IsAssetPendingOrApprovedAsync(d.AssetId, excludeId))
                            return false;
                    }
                    return true;
                })
                .WithMessage("This asset already has a pending transfer awaiting approval.")
                .When(x => x.AssetTransferHdr?.AssetTransferIssueDtl is { Count: > 0 } &&
                           x.AssetTransferHdr.AssetTransferIssueDtl.All(a => a.AssetId > 0));
        }
    }
}
