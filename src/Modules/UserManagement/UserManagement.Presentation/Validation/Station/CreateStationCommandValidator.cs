using UserManagement.Application.Station.Command.CreateStation;
using FluentValidation;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using UserManagement.Application.Common.Interfaces.IStation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Station
{
    public class CreateStationCommandValidator : AbstractValidator<CreateStationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStationQueryRepository _stationQueryRepository;

        public CreateStationCommandValidator(MaxLengthProvider maxLengthProvider, IStationQueryRepository stationQueryRepository)
        {
            var codeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Station>("Code") ?? 20;
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
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStationCommand.Code)} {rule.Error}");

                        RuleFor(x => x.StationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStationCommand.StationName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(codeMaxLength)
                            .WithMessage($"{nameof(CreateStationCommand.Code)} {rule.Error} {codeMaxLength}");

                        RuleFor(x => x.StationName)
                            .MaximumLength(nameMaxLength)
                            .WithMessage($"{nameof(CreateStationCommand.StationName)} {rule.Error} {nameMaxLength}");

                        RuleFor(x => x.Description)
                            .MaximumLength(descriptionMaxLength)
                            .WithMessage($"{nameof(CreateStationCommand.Description)} {rule.Error} {descriptionMaxLength}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.Code)
                            .MustAsync(async (code, cancellation) => !await _stationQueryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateStationCommand.Code)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Code));
                        break;

                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}
