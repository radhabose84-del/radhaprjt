using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Domain.Entities;
using QCManagement.Presentation.Validation.Common;

namespace QCManagement.Presentation.Validation.QcInspection
{
    public class SaveParameterCollectionCommandValidator : AbstractValidator<SaveParameterCollectionCommand>
    {
        public SaveParameterCollectionCommandValidator(
            IQcInspectionQueryRepository queryRepo,
            MaxLengthProvider maxLengthProvider)
        {
            var remarksMax = maxLengthProvider.GetMaxLength<QcInspectionDtl>("Remarks") ?? 500;

            RuleFor(x => x.QcInspectionHdrId)
                .GreaterThan(0).WithMessage("Inspection Id is required.");

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("Inspection not found.")
                .When(x => x.QcInspectionHdrId > 0);

            RuleFor(x => x.QcInspectionHdrId)
                .MustAsync(async (id, ct) => !await queryRepo.IsDisposedAsync(id))
                .WithMessage("Inspection has already been disposed.")
                .When(x => x.QcInspectionHdrId > 0);

            RuleFor(x => x.Parameters)
                .NotEmpty().WithMessage("At least one parameter result is required.");

            RuleForEach(x => x.Parameters).ChildRules(p =>
            {
                p.RuleFor(r => r.ActualValue)
                    .NotEmpty().WithMessage("Actual value is required.");
                p.RuleFor(r => r.Remarks)
                    .MaximumLength(remarksMax).WithMessage($"Remarks cannot exceed {remarksMax} characters.");
            });

            RuleForEach(x => x.Parameters)
                .MustAsync(async (cmd, p, ctx, ct) =>
                    await queryRepo.DetailBelongsToHeaderAsync(p.DetailId, cmd.QcInspectionHdrId))
                .WithMessage("Parameter row does not belong to this inspection.")
                .When(x => x.QcInspectionHdrId > 0);
        }
    }
}
