using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOffice
{
    public class UpdateSalesOfficeCommandValidator : AbstractValidator<UpdateSalesOfficeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOfficeQueryRepository _queryRepository;

        public UpdateSalesOfficeCommandValidator(
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
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOfficeName)} {rule.Error}");

                        RuleFor(x => x.SalesOrganisationId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOrganisationId)} {rule.Error}");

                        RuleFor(x => x.CityId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.CityId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesOfficeName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOfficeName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesOfficeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOfficeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ResponsibleManager)
                            .MaximumLength(maxLengthManager)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.ResponsibleManager)} {rule.Error} {maxLengthManager} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

                        RuleFor(x => x.RegionTerritory)
                            .MaximumLength(maxLengthRegion)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.RegionTerritory)} {rule.Error} {maxLengthRegion} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));

                        RuleFor(x => x.Address)
                            .MaximumLength(maxLengthAddress)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Address)} {rule.Error} {maxLengthAddress} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Address));

                        RuleFor(x => x.Pincode)
                            .MaximumLength(maxLengthPincode)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Pincode)} {rule.Error} {maxLengthPincode} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Pincode));

                        RuleFor(x => x.Phone)
                            .MaximumLength(maxLengthPhone)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Phone)} {rule.Error} {maxLengthPhone} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

                        RuleFor(x => x.Email)
                            .MaximumLength(maxLengthEmail)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Email)} {rule.Error} {maxLengthEmail} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Office {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOrganisationId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrganisationExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOrganisationId)} {rule.Error}")
                            .When(x => x.SalesOrganisationId > 0);

                        RuleFor(x => x.CityId)
                            .MustAsync(async (id, ct) => await _queryRepository.CityExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.CityId)} {rule.Error}")
                            .When(x => x.CityId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(cmd.SalesOfficeName!, cmd.SalesOrganisationId, cmd.Id!))
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.SalesOfficeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesOfficeName) && x.SalesOrganisationId > 0 && x.Id > 0);
                        break;

                    case "Pincode":
                        RuleFor(x => x.Pincode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Pincode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Pincode));
                        break;

                    case "Telephone":
                        RuleFor(x => x.Phone)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Phone)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
                        break;

                    case "Email":
                        RuleFor(x => x.Email)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.Email)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Email));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesOfficeCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
