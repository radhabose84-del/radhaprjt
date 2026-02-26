using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMaster
{
    public class DeleteDispatchAddressMasterCommandValidator : AbstractValidator<DeleteDispatchAddressMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMasterQueryRepository _queryRepository;

        public DeleteDispatchAddressMasterCommandValidator(IDispatchAddressMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteDispatchAddressMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Dispatch Address Master {rule.Error}");
                        break;

                    case "SoftDelete":
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
