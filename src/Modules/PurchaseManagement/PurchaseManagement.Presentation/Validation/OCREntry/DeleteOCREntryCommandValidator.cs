using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry;

namespace PurchaseManagement.Presentation.Validation.OCREntry
{
    public class DeleteOCREntryCommandValidator : AbstractValidator<DeleteOCREntryCommand>
    {
        public DeleteOCREntryCommandValidator(IOCREntryQueryRepository queryRepo)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.")
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("OCR not found.")
                .MustAsync(async (id, ct) => await queryRepo.IsEditableAsync(id))
                .WithMessage("This OCR is approved/converted and cannot be deleted.")
                .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                .WithMessage("This master is linked with other records. You cannot delete this record.")
                .When(x => x.Id > 0);
        }
    }
}
