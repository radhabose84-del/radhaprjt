using FluentValidation;
using PurchaseManagement.Application.BlanketMaster.Commands.Update;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;

namespace PurchaseManagement.Presentation.Validation.BlanketMaster;

public sealed class UpdateBlanketMasterCommandValidator : AbstractValidator<UpdateBlanketMasterCommand>
{
    public UpdateBlanketMasterCommandValidator(IBlanketMasterQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id, ct))
                    .WithMessage("Blanket Master not found.");
            });

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("VendorId is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("CurrencyId is required.");

        RuleFor(x => x.ProcurementTypeId)
            .GreaterThan(0).WithMessage("ProcurementTypeId is required.");

        RuleFor(x => x.ValidityFrom)
            .NotEmpty().WithMessage("ValidityFrom is required.");

        RuleFor(x => x.ValidityTo)
            .NotEmpty().WithMessage("ValidityTo is required.")
            .GreaterThanOrEqualTo(x => x.ValidityFrom)
            .WithMessage("ValidityTo must be greater than or equal to ValidityFrom.");

        RuleFor(x => x.IsActive)
            .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");

        RuleFor(x => x.Details)
            .NotNull().WithMessage("Details are required.")
            .Must(d => d != null && d.Count > 0)
            .WithMessage("At least one detail line is required.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Id <= 0 || cmd.VendorId <= 0 || cmd.Details == null || cmd.Details.Count == 0)
                    return true;

                var itemIds = cmd.Details
                    .Where(d => d.ItemId > 0)
                    .Select(d => d.ItemId)
                    .Distinct()
                    .ToList();

                if (itemIds.Count == 0)
                    return true;

                return !await queryRepo.HasOverlappingBlanketAsync(
                    cmd.VendorId, itemIds, cmd.ValidityFrom, cmd.ValidityTo, cmd.Id);
            })
            .WithMessage("A blanket agreement already exists for the same vendor and item(s) within the specified validity period.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ItemId)
                .GreaterThan(0).WithMessage("ItemId must be greater than zero.");

            detail.RuleFor(d => d.UOMId)
                .GreaterThan(0).WithMessage("UOMId must be greater than zero.");

            detail.RuleFor(d => d.EstimatedQuantity)
                .GreaterThan(0).WithMessage("EstimatedQuantity must be greater than zero.");

            detail.RuleFor(d => d.Rate)
                .GreaterThan(0).WithMessage("Rate must be greater than zero.");

            detail.RuleForEach(d => d.Schedules).ChildRules(schedule =>
            {
                schedule.RuleFor(s => s.ScheduleDate)
                    .NotEmpty().WithMessage("ScheduleDate is required.");

                schedule.RuleFor(s => s.ScheduleQuantity)
                    .GreaterThan(0).WithMessage("ScheduleQuantity must be greater than zero.");
            });
        });
    }
}
