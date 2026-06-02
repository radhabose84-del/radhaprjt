using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Domain.Entities;
using QCManagement.Presentation.Validation.Common;

namespace QCManagement.Presentation.Validation.QcInspection
{
    public class SaveDispositionCommandValidator : AbstractValidator<SaveDispositionCommand>
    {
        public SaveDispositionCommandValidator(
            IQcInspectionQueryRepository queryRepo,
            MaxLengthProvider maxLengthProvider)
        {
            var remarksMax = maxLengthProvider.GetMaxLength<QcInspectionHdr>("DispositionRemarks") ?? 500;

            RuleFor(x => x.QcInspectionHdrId)
                .GreaterThan(0).WithMessage("Inspection Id is required.");

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("Inspection not found.")
                .When(x => x.QcInspectionHdrId > 0);

            // Disposition is re-editable (Hold→Approve, Reject→CAP, etc.) — no "already disposed" lock.

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => await queryRepo.AllParametersEvaluatedAsync(id))
                .WithMessage("All parameters must be evaluated before disposition.")
                .When(x => x.QcInspectionHdrId > 0);

            RuleFor(x => x.QcStatusId)
                .GreaterThan(0).WithMessage("QC Status is required.");

            RuleFor(x => x.QcStatusId)
                .MustAsync(async (id, ct) => await queryRepo.QcStatusIdExistsAsync(id))
                .WithMessage("Invalid QC Status.")
                .When(x => x.QcStatusId > 0);

            RuleFor(x => x.AcceptedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Accepted quantity cannot be negative.");
            RuleFor(x => x.RejectedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Rejected quantity cannot be negative.");

            // Accepted + Rejected must equal Received (±0.001)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var recv = await queryRepo.GetReceivedQuantityAsync(cmd.QcInspectionHdrId);
                    if (recv == null) return true; // existence handled above
                    return Math.Abs((cmd.AcceptedQuantity + cmd.RejectedQuantity) - recv.Value) <= 0.001m;
                })
                .WithMessage("Accepted + Rejected must equal Received Quantity.")
                .When(x => x.QcInspectionHdrId > 0);

            // ── Status-specific rules ──
            // The status code is resolved from QcStatusId via an async lookup, so each rule is a
            // MustAsync that no-ops for any status it does not apply to.

            // APR — no critical failure
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (!await IsStatusAsync(queryRepo, cmd.QcStatusId, "APR")) return true;
                    return !await queryRepo.HasCriticalFailureAsync(cmd.QcInspectionHdrId);
                })
                .WithMessage("Cannot approve — critical parameter(s) failed.")
                .When(x => x.QcInspectionHdrId > 0 && x.QcStatusId > 0);

            // APR — zero rejected
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await IsStatusAsync(queryRepo, cmd.QcStatusId, "APR") || cmd.RejectedQuantity == 0m)
                .WithMessage("Approved status requires zero rejected quantity.")
                .When(x => x.QcStatusId > 0);

            // RJC — zero accepted
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await IsStatusAsync(queryRepo, cmd.QcStatusId, "RJC") || cmd.AcceptedQuantity == 0m)
                .WithMessage("Rejected status requires zero accepted quantity.")
                .When(x => x.QcStatusId > 0);

            // HLD — both zero
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await IsStatusAsync(queryRepo, cmd.QcStatusId, "HLD")
                    || (cmd.AcceptedQuantity == 0m && cmd.RejectedQuantity == 0m))
                .WithMessage("Hold status requires zero accepted and zero rejected quantity.")
                .When(x => x.QcStatusId > 0);

            // CAP — mandatory remarks (min 10)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (!await IsStatusAsync(queryRepo, cmd.QcStatusId, "CAP")) return true;
                    return !string.IsNullOrWhiteSpace(cmd.DispositionRemarks)
                           && cmd.DispositionRemarks.Trim().Length >= 10;
                })
                .WithMessage("Disposition remarks (min 10 chars) are mandatory for Conditionally Approved.")
                .When(x => x.QcStatusId > 0);

            RuleFor(x => x.DispositionRemarks)
                .MaximumLength(remarksMax).WithMessage($"Disposition remarks cannot exceed {remarksMax} characters.")
                .When(x => !string.IsNullOrEmpty(x.DispositionRemarks));
        }

        private static async Task<bool> IsStatusAsync(
            IQcInspectionQueryRepository queryRepo, int qcStatusId, string code)
        {
            var resolved = await queryRepo.GetQcStatusCodeByIdAsync(qcStatusId);
            return string.Equals((resolved ?? string.Empty).Trim(), code, StringComparison.OrdinalIgnoreCase);
        }
    }
}
