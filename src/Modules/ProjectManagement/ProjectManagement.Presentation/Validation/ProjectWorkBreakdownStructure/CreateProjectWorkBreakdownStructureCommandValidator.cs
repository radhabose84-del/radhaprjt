using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using FluentValidation;

namespace ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure
{
    public class CreateProjectWorkBreakdownStructureCommandValidator  : AbstractValidator<CreateProjectWorkBreakdownStructureCommand>
    {

      public CreateProjectWorkBreakdownStructureCommandValidator(            IProjectWorkBreakdownStructureCommandRepository repo)
        {
            // Project must be selected
            RuleFor(x => x.ProjectId)
                .GreaterThan(0)
                .WithMessage("Project is required.");

            // WBS Name
            RuleFor(x => x.WorkBreakdownStructureName)
                .NotEmpty().WithMessage("WBS Name is required.")
                .MaximumLength(200).WithMessage("WBS Name cannot exceed 200 characters.")
                .Matches(@"^[a-zA-Z0-9 ]+$")
                    .WithMessage("WBS Name allows only alphabets, numbers and spaces.")
                .MustAsync(async (cmd, name, ct) =>
                    !await repo.NameExistsAsync(cmd.ProjectId, name))
                    .WithMessage("WBS Name must be unique within the same project.");

            // Responsible Department
            RuleFor(x => x.ResponsibleDepartmentId)
                .GreaterThan(0)
                .WithMessage("Responsible Department is required.");

            // Responsible Person
            RuleFor(x => x.ResponsiblePerson)
                .NotEmpty().WithMessage("Responsible Person is required.")
                .MaximumLength(200).WithMessage("Responsible Person cannot exceed 200 characters.");

            // Currency
            RuleFor(x => x.CurrencyId)
                .GreaterThan(0)
                .WithMessage("Currency is required.");

            // Start <= End (if both are present)
            RuleFor(x => x.StartDate)
                .Must((cmd, start) =>
                {
                    if (!start.HasValue || !cmd.EndDate.HasValue)
                        return true;

                    return start.Value <= cmd.EndDate.Value;
                })
                .WithMessage("Start Date cannot be greater than End Date.");

            // Milestone rules
            When(x => x.IsMilestone, () =>
            {
                RuleFor(x => x.MilestoneDate)
                    .NotNull().WithMessage("Milestone Date is required when Milestone Flag is true.");

                RuleFor(x => x)
                    .Must(x =>
                    {
                        if (!x.MilestoneDate.HasValue || !x.StartDate.HasValue || !x.EndDate.HasValue)
                            return true; // other rules will handle missing dates

                        return x.MilestoneDate.Value >= x.StartDate.Value &&
                               x.MilestoneDate.Value <= x.EndDate.Value;
                    })
                    .WithMessage("Milestone Date must be between Start Date and End Date.");
            });

            // Budget
            RuleFor(x => x.PlannedBudgetAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PlannedBudgetAmount.HasValue)
                .WithMessage("Planned Budget Amount must be positive.");
        }
    }
}