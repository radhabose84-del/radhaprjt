using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ProfitCentre
{
    public class DeleteProfitCentreCommandValidator : AbstractValidator<DeleteProfitCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProfitCentreQueryRepository _queryRepository;

        public DeleteProfitCentreCommandValidator(IProfitCentreQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteProfitCentreCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Profit Centre {rule.Error}");
                        break;

                    case "SoftDelete":
                        // Blocks delete when the node has children or current-year transactions (AC#5).
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
