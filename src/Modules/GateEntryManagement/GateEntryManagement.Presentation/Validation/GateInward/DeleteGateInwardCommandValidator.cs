using FluentValidation;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInward;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.GateInward
{
    public class DeleteGateInwardCommandValidator : AbstractValidator<DeleteGateInwardCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGateInwardQueryRepository _queryRepository;

        public DeleteGateInwardCommandValidator(IGateInwardQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty().WithMessage($"{nameof(DeleteGateInwardCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Gate Inward Entry {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
