using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Delete;

namespace PurchaseManagement.Presentation.Validation.DutyMaster
{
    public sealed class DeleteDutyMasterValidator : AbstractValidator<DeleteDutyMasterCommand>
    {
        public DeleteDutyMasterValidator(IDutyMasterQueryRepository queryRepo)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid Id is required.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id, ct))
                        .WithMessage("Duty Master not found.");

                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                        .WithMessage("This master is linked with other records. You cannot delete this record.");
                });
        }
    }
}
