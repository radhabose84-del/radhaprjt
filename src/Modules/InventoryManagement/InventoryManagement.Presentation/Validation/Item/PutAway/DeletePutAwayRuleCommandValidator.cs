using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.Item.PutAway
{
    public class DeletePutAwayRuleCommandValidator : AbstractValidator<DeletePutAwayRuleCommand>
    {
        private readonly IPutAwayRuleCommandRepository _ruleRepo;

        public DeletePutAwayRuleCommandValidator(IPutAwayRuleCommandRepository ruleRepo)
        {
            _ruleRepo = ruleRepo;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage($"{nameof(DeletePutAwayRuleCommand.Id)} is required.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _ruleRepo.ExistsAsync(id, ct))
                        .WithMessage("PutAway Rule not found.");
                });
        }
    }
}
