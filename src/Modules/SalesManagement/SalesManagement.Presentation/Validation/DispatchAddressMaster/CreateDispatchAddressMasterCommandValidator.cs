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

                    case "Pincode":
                        RuleFor(x => x.PinCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.PinCode)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PinCode));
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.MobileNumber)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.MobileNumber)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "Email":
                        RuleFor(x => x.Email)
                            .EmailAddress()
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Email)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "GstFormat":
                        RuleFor(x => x.GSTIN)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.GSTIN)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GSTIN));
                        break;

                    case "Latitude":
                        RuleFor(x => x.Latitude)
                            .InclusiveBetween(-90m, 90m)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Latitude)}{rule.Error}")
                            .When(x => x.Latitude.HasValue);
                        break;

                    case "Longitude":
                        RuleFor(x => x.Longitude)
                            .InclusiveBetween(-180m, 180m)
                            .WithMessage($"{nameof(CreateDispatchAddressMasterCommand.Longitude)}{rule.Error}")
                            .When(x => x.Longitude.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
