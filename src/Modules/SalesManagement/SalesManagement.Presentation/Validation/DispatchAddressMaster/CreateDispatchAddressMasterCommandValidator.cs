using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMaster
{
    public class CreateDispatchAddressMasterCommandValidator : AbstractValidator<CreateDispatchAddressMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMasterQueryRepository _queryRepo;

        public CreateDispatchAddressMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDispatchAddressMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName        = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("DispatchAddressName") ?? 150;
            var maxLengthLine1       = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("AddressLine1") ?? 250;
            var maxLengthLine2       = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("AddressLine2") ?? 250;
            var maxLengthContact     = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("ContactPerson") ?? 120;

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
                        RuleFor(x => x.DispatchAddressName)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}");

                        RuleFor(x => x.AddressLine1)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.AddressLine1)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.AddressLine1)} {rule.Error}");

                        RuleFor(x => x.PinCode)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.PinCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.PinCode)} {rule.Error}");

                        RuleFor(x => x.CityId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CityId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CityId)} {rule.Error}");

                        RuleFor(x => x.StateId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.StateId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.StateId)} {rule.Error}");

                        RuleFor(x => x.CountryId)
                            .NotNull().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CountryId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CountryId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.DispatchAddressName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.AddressLine1)
                            .MaximumLength(maxLengthLine1)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.AddressLine1)} {rule.Error} {maxLengthLine1} characters.");

                        RuleFor(x => x.AddressLine2)
                            .MaximumLength(maxLengthLine2)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.AddressLine2)} {rule.Error} {maxLengthLine2} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

                        RuleFor(x => x.ContactPerson)
                            .MaximumLength(maxLengthContact)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.ContactPerson)} {rule.Error} {maxLengthContact} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ContactPerson));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CityId)
                            .MustAsync(async (cityId, ct) => await _queryRepo.CityExistsAsync(cityId))
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId > 0);

                        RuleFor(x => x.StateId)
                            .MustAsync(async (stateId, ct) => await _queryRepo.StateExistsAsync(stateId))
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.StateId)} {rule.Error}")
                            .When(x => x.StateId > 0);

                        RuleFor(x => x.CountryId)
                            .MustAsync(async (countryId, ct) => await _queryRepo.CountryExistsAsync(countryId))
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.CountryId)} {rule.Error}")
                            .When(x => x.CountryId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.DispatchAddressName)
                            .MustAsync(async (command, name, ct) =>
                                !await _queryRepo.CompositeKeyExistsAsync(command.DispatchAddressName!, command.CityId, command.PinCode!))
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.DispatchAddressName) && x.CityId > 0 && !string.IsNullOrWhiteSpace(x.PinCode));
                        break;

                    default:
                        break;
                }
            }

            // ── Custom field-specific business rules ────────────────────────
            RuleFor(x => x.PinCode)
                .Matches(@"^\d{6}$")
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.PinCode)} must be a 6-digit numeric value.")
                .When(x => !string.IsNullOrWhiteSpace(x.PinCode));

            RuleFor(x => x.MobileNumber)
                .Matches(@"^\d{10}$")
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.MobileNumber)} must be a 10-digit numeric value.")
                .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Email)} must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.GSTIN)
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.GSTIN)} must be a valid 15-character GSTIN format.")
                .When(x => !string.IsNullOrWhiteSpace(x.GSTIN));

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90m, 90m)
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Latitude)} must be between -90 and 90.")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180m, 180m)
                .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Longitude)} must be between -180 and 180.")
                .When(x => x.Longitude.HasValue);
        }
    }
}
