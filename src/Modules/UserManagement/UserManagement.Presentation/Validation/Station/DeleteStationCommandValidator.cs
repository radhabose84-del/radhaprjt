using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Command.DeleteStation;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Station
{
    public class DeleteStationCommandValidator : AbstractValidator<DeleteStationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStationQueryRepository _stationQueryRepository;

        public DeleteStationCommandValidator(IStationQueryRepository stationQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _stationQueryRepository = stationQueryRepository;
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
                            .WithMessage($"{nameof(DeleteStationCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => !await _stationQueryRepository.NotFoundAsync(id))
                            .WithMessage($"Station {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
