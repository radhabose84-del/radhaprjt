using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using FluentValidation;

namespace BudgetManagement.Presentation.Validation.BudgetGroup;

    public class DeleteBudgetGroupCommandValidator : AbstractValidator<DeleteBudgetGroupCommand>
    {
        public DeleteBudgetGroupCommandValidator(IBudgetGroupQueryRepository queryRepo)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required.")
                
                .MustAsync(async (id, ct) => (await queryRepo.GetByIdAsync(id)) != null)
                .WithMessage("BudgetGroup not found.")
                
                .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidation(id, ct))
                .WithMessage("Cannot delete BudgetGroup as it has child BudgetGroups.");
        }
    }

