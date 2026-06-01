using FluentValidation;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Commands.DeleteQcInspection;

namespace QCManagement.Presentation.Validation.QcInspection
{
    public class DeleteQcInspectionCommandValidator : AbstractValidator<DeleteQcInspectionCommand>
    {
        public DeleteQcInspectionCommandValidator(IQcInspectionQueryRepository queryRepo)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.");

            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("QC Inspection not found.")
                .When(x => x.Id > 0);

            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => !await queryRepo.IsDisposedAsync(id))
                .WithMessage("Cannot delete a disposed inspection.")
                .When(x => x.Id > 0);
        }
    }
}
