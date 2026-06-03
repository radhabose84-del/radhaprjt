using UserManagement.Application.Station.Command.UpdateStation;
using FluentValidation;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using UserManagement.Application.Common.Interfaces.IStation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Station
{
    public class UpdateStationCommandValidator : AbstractValidator<UpdateStationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStationQueryRepository _stationQueryRepository;

        public UpdateStationCommandValidator(MaxLengthProvider maxLengthProvider, IStationQueryRepository stationQueryRepository)
        {
            var nameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Station>("StationName") ?? 100;
            var descriptionMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Station>("Description") ?? 250;

            _stationQueryRepository = stationQueryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.StationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateStationCommand.StationName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.StationName)
                            .MaximumLength(nameMaxLength)
                            .WithMessage($"{nameof(UpdateStationCommand.StationName)} {rule.Error} {nameMaxLength}");

                        RuleFor(x => x.Description)
                            .MaximumLength(descriptionMaxLength)
                            .WithMessage($"{nameof(UpdateStationCommand.Description)} {rule.Error} {descriptionMaxLength}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, cancellation) => !await _stationQueryRepository.NotFoundAsync(id))
                            .WithMessage($"Station {rule.Error}");
                        break;

                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}
