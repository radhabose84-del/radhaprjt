using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.Feeder
{
    public class DeleteFeederCommandValidator : AbstractValidator<DeleteFeederCommand>
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly IFeederQueryRepository _feederQueryRepository;

        public DeleteFeederCommandValidator(IFeederQueryRepository feederGroupQueryRepository, MaxLengthProvider maxLengthProvider)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _feederQueryRepository = feederGroupQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
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
                            .WithMessage($"{nameof(DeleteFeederCommand.Id)} {rule.Error}");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) =>
                            await _feederQueryRepository.NotFoundAsync(Id))
                            .WithName("Feeder Id")
                            .WithMessage($"{rule.Error}"); break;
                    default:
                        break;
                }
            }

        }

        
    }
}