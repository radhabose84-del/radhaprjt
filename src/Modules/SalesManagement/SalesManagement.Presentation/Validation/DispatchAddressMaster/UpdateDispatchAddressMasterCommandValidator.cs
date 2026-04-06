using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAddressMaster
{
    public class UpdateDispatchAddressMasterCommandValidator : AbstractValidator<UpdateDispatchAddressMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAddressMasterQueryRepository _queryRepo;

        public UpdateDispatchAddressMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDispatchAddressMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName    = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("DispatchAddressName") ?? 150;
            var maxLengthLine1   = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("AddressLine1") ?? 250;
            var maxLengthLine2   = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("AddressLine2") ?? 250;
            var maxLengthContact = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.DispatchAddressMaster>("ContactPerson") ?? 120;

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
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}");

                        RuleFor(x => x.AddressLine1)
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.AddressLine1)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.AddressLine1)} {rule.Error}");

                        RuleFor(x => x.PinCode)
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.PinCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.PinCode)} {rule.Error}");

                        RuleFor(x => x.CityId)
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CityId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CityId)} {rule.Error}");

                        RuleFor(x => x.StateId)
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.StateId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.StateId)} {rule.Error}");

                        RuleFor(x => x.CountryId)
                            .NotNull().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CountryId)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CountryId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.DispatchAddressName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.AddressLine1)
                            .MaximumLength(maxLengthLine1)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.AddressLine1)} {rule.Error} {maxLengthLine1} characters.");

                        RuleFor(x => x.AddressLine2)
                            .MaximumLength(maxLengthLine2)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.AddressLine2)} {rule.Error} {maxLengthLine2} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

                        RuleFor(x => x.ContactPerson)
                            .MaximumLength(maxLengthContact)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.ContactPerson)} {rule.Error} {maxLengthContact} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ContactPerson));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Dispatch Address Master {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CityId)
                            .MustAsync(async (cityId, ct) => await _queryRepo.CityExistsAsync(cityId))
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId > 0);

                        RuleFor(x => x.StateId)
                            .MustAsync(async (stateId, ct) => await _queryRepo.StateExistsAsync(stateId))
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.StateId)} {rule.Error}")
                            .When(x => x.StateId > 0);

                        RuleFor(x => x.CountryId)
                            .MustAsync(async (countryId, ct) => await _queryRepo.CountryExistsAsync(countryId))
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.CountryId)} {rule.Error}")
                            .When(x => x.CountryId > 0);

                        RuleFor(x => x.FreightId)
                            .MustAsync(async (id, ct) => await _queryRepo.FreightMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.FreightId)} {rule.Error}")
                            .When(x => x.FreightId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.DispatchAddressName)
                            .MustAsync(async (command, name, ct) =>
                                !await _queryRepo.CompositeKeyExistsAsync(command.DispatchAddressName!, command.CityId, command.PinCode!, command.Id))
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.DispatchAddressName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.DispatchAddressName) && x.CityId > 0 && !string.IsNullOrWhiteSpace(x.PinCode));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.IsActive)} {rule.Error}");
                        break;

                    case "Pincode":
                        RuleFor(x => x.PinCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.PinCode)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PinCode));
                        break;

                    case "MobileNumber":
                        RuleFor(x => x.MobileNumber)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.MobileNumber)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));
                        break;

                    case "Email":
                        RuleFor(x => x.Email)
                            .EmailAddress()
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.Email)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "GstFormat":
                        RuleFor(x => x.GSTIN)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.GSTIN)}{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GSTIN));
                        break;

                    case "Latitude":
                        RuleFor(x => x.Latitude)
                            .InclusiveBetween(-90m, 90m)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.Latitude)}{rule.Error}")
                            .When(x => x.Latitude.HasValue);
                        break;

                    case "Longitude":
                        RuleFor(x => x.Longitude)
                            .InclusiveBetween(-180m, 180m)
                            .WithMessage($"{nameof(UpdateDispatchAddressMasterCommand.Longitude)}{rule.Error}")
                            .When(x => x.Longitude.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
