using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.GatePass
{
    public class DeleteGatePassCommandValidator : AbstractValidator<DeleteGatePassCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGatePassQueryRepository _queryRepository;

        public DeleteGatePassCommandValidator(IGatePassQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteGatePassCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Gate Pass {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
