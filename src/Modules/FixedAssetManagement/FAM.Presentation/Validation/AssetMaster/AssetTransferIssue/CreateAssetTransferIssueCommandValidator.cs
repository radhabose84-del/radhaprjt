using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;

namespace FAM.Presentation.Validation.AssetMaster.AssetTransferIssue
{
    public class CreateAssetTransferIssueCommandValidator : AbstractValidator<CreateAssetTransferIssueCommand>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;

        public CreateAssetTransferIssueCommandValidator(IAssetTransferQueryRepository assetTransferQueryRepository)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;

            RuleFor(x => x.AssetTransferIssueHdrDto!.DocDate)
                .NotEmpty().WithMessage("Document Date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Document Date cannot be in the future.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.FromUnitId)
                .GreaterThan(0).WithMessage("From Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.ToUnitId)
                .GreaterThan(0).WithMessage("To Unit ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.FromDepartmentId)
                .GreaterThan(0).WithMessage("From Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.ToDepartmentId)
                .GreaterThan(0).WithMessage("To Department ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.FromCustodianId)
                .GreaterThan(0).WithMessage("From Custodian ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.ToCustodianId)
                .GreaterThan(0).WithMessage("To Custodian ID must be greater than 0.");

            RuleFor(x => x.AssetTransferIssueHdrDto!.AssetTransferIssueDtls)
                .NotEmpty().WithMessage("At least one asset must be included.")
                .Must(AssetTransferIssueDtls => AssetTransferIssueDtls!.All(a => a.AssetId > 0))
                .WithMessage("All assets must have a valid Asset ID.")
                .Must(AssetTransferIssueDtls => AssetTransferIssueDtls!.All(a => a.AssetValue > 0))
                .WithMessage("All assets must have a valid Asset Value.");

            // SCRUM-1463: within-payload duplicate — same AssetId listed twice in the same submission
            RuleFor(x => x.AssetTransferIssueHdrDto!.AssetTransferIssueDtls)
                .Must(dtls => dtls!.Select(d => d.AssetId).Distinct().Count() == dtls!.Count)
                .WithMessage("The same asset cannot appear more than once in the transfer.")
                .When(x => x.AssetTransferIssueHdrDto?.AssetTransferIssueDtls is { Count: > 0 });

            // SCRUM-1463: cross-payload duplicate — any of the listed assets already has a
            // Pending or Approved-but-not-acknowledged transfer awaiting approval.
            RuleFor(x => x.AssetTransferIssueHdrDto!.AssetTransferIssueDtls)
                .MustAsync(async (dtls, ct) =>
                {
                    foreach (var d in dtls!)
                    {
                        if (await _assetTransferQueryRepository.IsAssetPendingOrApprovedAsync(d.AssetId))
                            return false;
                    }
                    return true;
                })
                .WithMessage("This asset already has a pending transfer awaiting approval.")
                .When(x => x.AssetTransferIssueHdrDto?.AssetTransferIssueDtls is { Count: > 0 } &&
                           x.AssetTransferIssueHdrDto.AssetTransferIssueDtls.All(a => a.AssetId > 0));
        }
    }
}
