using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOffice
{
    public class CreateSalesOfficeCommandValidator : AbstractValidator<CreateSalesOfficeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOfficeQueryRepository _queryRepository;

        public CreateSalesOfficeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOfficeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName    = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("SalesOfficeName")    ?? 100;
            var maxLengthManager = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("ResponsibleManager") ?? 100;
            var maxLengthRegion  = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("RegionTerritory")    ?? 100;
            var maxLengthAddress = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("Address")            ?? 500;
            var maxLengthPincode = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("Pincode")            ?? 20;
            var maxLengthPhone   = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("Phone")              ?? 20;
            var maxLengthEmail   = maxLengthProvider.GetMaxLength<Domain.Entities.SalesOffice>("Email")              ?? 200;

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
                        RuleFor(x => x.SalesOfficeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOfficeName)} {rule.Error}");

                        RuleFor(x => x.SalesOrganisationId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOrganisationId)} {rule.Error}");

                        RuleFor(x => x.CityId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.CityId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesOfficeName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOfficeName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesOfficeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOfficeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ResponsibleManager)
                            .MaximumLength(maxLengthManager)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.ResponsibleManager)} {rule.Error} {maxLengthManager} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

                        RuleFor(x => x.RegionTerritory)
                            .MaximumLength(maxLengthRegion)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.RegionTerritory)} {rule.Error} {maxLengthRegion} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));

                        RuleFor(x => x.Address)
                            .MaximumLength(maxLengthAddress)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Address)} {rule.Error} {maxLengthAddress} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Address));

                        RuleFor(x => x.Pincode)
                            .MaximumLength(maxLengthPincode)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Pincode)} {rule.Error} {maxLengthPincode} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Pincode));

                        RuleFor(x => x.Phone)
                            .MaximumLength(maxLengthPhone)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Phone)} {rule.Error} {maxLengthPhone} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

                        RuleFor(x => x.Email)
                            .MaximumLength(maxLengthEmail)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Email)} {rule.Error} {maxLengthEmail} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOrganisationId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrganisationExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOrganisationId)} {rule.Error}")
                            .When(x => x.SalesOrganisationId > 0);

                        RuleFor(x => x.CityId)
                            .MustAsync(async (id, ct) => await _queryRepository.CityExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(cmd.SalesOfficeName!, cmd.SalesOrganisationId!))
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOfficeName) && x.SalesOrganisationId > 0);
                        break;

                    case "Pincode":
                        RuleFor(x => x.Pincode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Pincode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Pincode));
                        break;

                    case "Telephone":
                        RuleFor(x => x.Phone)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Phone)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
                        break;

                    case "Email":
                        RuleFor(x => x.Email)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesOfficeCommand.Email)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
