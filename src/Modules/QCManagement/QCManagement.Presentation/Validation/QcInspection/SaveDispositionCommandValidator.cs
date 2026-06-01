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

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => !await queryRepo.IsDisposedAsync(id))
                .WithMessage("This inspection has already been disposed.")
                .When(x => x.QcInspectionHdrId > 0);

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => await queryRepo.AllParametersEvaluatedAsync(id))
                .WithMessage("All parameters must be evaluated before disposition.")
                .When(x => x.QcInspectionHdrId > 0);

            RuleFor(x => x.QcStatusCode)
                .NotEmpty().WithMessage("QC Status is required.");

            RuleFor(x => x.QcStatusCode)
                .MustAsync(async (code, ct) => await queryRepo.QcStatusCodeExistsAsync(code!.Trim().ToUpperInvariant()))
                .WithMessage("Invalid QC Status.")
                .When(x => !string.IsNullOrWhiteSpace(x.QcStatusCode));

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

            // APR — no critical failure
            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => !await queryRepo.HasCriticalFailureAsync(id))
                .WithMessage("Cannot approve — critical parameter(s) failed.")
                .When(x => Code(x) == "APR" && x.QcInspectionHdrId > 0);

            // APR — zero rejected
            RuleFor(x => x.RejectedQuantity)
                .Equal(0m).WithMessage("Approved status requires zero rejected quantity.")
                .When(x => Code(x) == "APR");

            // RJC — zero accepted
            RuleFor(x => x.AcceptedQuantity)
                .Equal(0m).WithMessage("Rejected status requires zero accepted quantity.")
                .When(x => Code(x) == "RJC");

            // HLD — both zero
            RuleFor(x => x)
                .Must(x => x.AcceptedQuantity == 0m && x.RejectedQuantity == 0m)
                .WithMessage("Hold status requires zero accepted and zero rejected quantity.")
                .When(x => Code(x) == "HLD");

            // CAP — mandatory remarks (min 10)
            RuleFor(x => x.DispositionRemarks)
                .NotEmpty().MinimumLength(10)
                .WithMessage("Disposition remarks (min 10 chars) are mandatory for Conditionally Approved.")
                .When(x => Code(x) == "CAP");

            RuleFor(x => x.DispositionRemarks)
                .MaximumLength(remarksMax).WithMessage($"Disposition remarks cannot exceed {remarksMax} characters.")
                .When(x => !string.IsNullOrEmpty(x.DispositionRemarks));
        }

        private static string Code(SaveDispositionCommand x) =>
            (x.QcStatusCode ?? string.Empty).Trim().ToUpperInvariant();
    }
}
