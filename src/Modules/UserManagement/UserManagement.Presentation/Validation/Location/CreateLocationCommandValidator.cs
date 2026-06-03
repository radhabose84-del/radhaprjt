using UserManagement.Application.Location.Command.CreateLocation;
using FluentValidation;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using UserManagement.Application.Common.Interfaces.ILocation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Location
{
    public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ILocationQueryRepository _locationQueryRepository;

        public CreateLocationCommandValidator(MaxLengthProvider maxLengthProvider, ILocationQueryRepository locationQueryRepository)
        {
            var codeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Location>("Code") ?? 20;
            var nameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Location>("LocationName") ?? 100;
            var descriptionMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Location>("Description") ?? 250;

            _locationQueryRepository = locationQueryRepository;
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
                            .WithMessage($"{nameof(CreateLocationCommand.Code)} {rule.Error}");

                        RuleFor(x => x.LocationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateLocationCommand.LocationName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(codeMaxLength)
                            .WithMessage($"{nameof(CreateLocationCommand.Code)} {rule.Error} {codeMaxLength}");

                        RuleFor(x => x.LocationName)
                            .MaximumLength(nameMaxLength)
                            .WithMessage($"{nameof(CreateLocationCommand.LocationName)} {rule.Error} {nameMaxLength}");

                        RuleFor(x => x.Description)
                            .MaximumLength(descriptionMaxLength)
                            .WithMessage($"{nameof(CreateLocationCommand.Description)} {rule.Error} {descriptionMaxLength}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.Code)
                            .MustAsync(async (code, cancellation) => !await _locationQueryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateLocationCommand.Code)} {rule.Error}")
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
