using UserManagement.API.Validation.Common;
using UserManagement.Application.Companies.Commands.CreateCompany;
using UserManagement.Domain.Entities;
using FluentValidation;
using UserManagement.Application.Common.Interfaces.ICompany;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Companies
{
    public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICompanyQueryRepository _companyRepository;

        public CreateCompanyCommandValidator(MaxLengthProvider maxLengthProvider, ICompanyQueryRepository companyService)
        {
            var companyNameMaxLength = maxLengthProvider.GetMaxLength<Company>("CompanyName") ?? 50;
            var LegalNameMaxLength = maxLengthProvider.GetMaxLength<Company>("LegalName") ?? 50;


            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _companyRepository = companyService;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":

                        RuleFor(x => x.Company.CompanyName)
                       .NotEmpty()
                       .WithMessage($"{rule.Error}");

                        RuleFor(x => x.Company.LegalName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.LegalName)} {rule.Error}");

                        RuleFor(x => x.Company.GstNumber)
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.GstNumber)} {rule.Error}");

                        RuleFor(x => x.Company.YearOfEstablishment)
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.YearOfEstablishment)} {rule.Error}");

                        RuleFor(x => x.Company.Website)
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.Website)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyAddress.AddressLine1)
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.AddressLine1)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyAddress.PinCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.AddressLine2)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyContact.Name)
                     .NotEmpty()
                     .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Name)} {rule.Error}");
                        RuleFor(x => x.Company.CompanyContact.Designation)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Designation)} {rule.Error}");
                        RuleFor(x => x.Company.CompanyContact.Email)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Email)} {rule.Error}");
                        RuleFor(x => x.Company.CompanyContact.Phone)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Phone)} {rule.Error}");
                        RuleFor(x => x.Company.PanNumber)
                        .NotEmpty()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.PanNumber)} {rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Company.CompanyName)
                            .MaximumLength(companyNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyName)} {rule.Error} {companyNameMaxLength}");
                        RuleFor(x => x.Company.LegalName)
                            .MaximumLength(LegalNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.LegalName)} {rule.Error} {LegalNameMaxLength}");
                        break;

                    case "GstFormat":
                        RuleFor(x => x.Company.GstNumber)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.GstNumber)} {rule.Error}");
                        break;
                    case "NumericOnly":
                        RuleFor(x => x.Company.YearOfEstablishment)
                            .InclusiveBetween(1900, DateTime.Now.Year)
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.YearOfEstablishment)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyAddress.CityId)
                  .InclusiveBetween(1, int.MaxValue)
                  .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.CityId)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyAddress.StateId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.StateId)} {rule.Error}");

                        RuleFor(x => x.Company.CompanyAddress.CountryId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.CountryId)} {rule.Error}");
                        break;

                    case "Website":
                        RuleFor(x => x.Company.Website)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.Website)} {rule.Error}");
                        break;
                    case "MinLength":
                        RuleFor(x => x.Company.EntityId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateCompanyCommand.Company.EntityId)} {rule.Error} {0}");
                        break;

                    case "Pincode":
                        RuleFor(x => x.Company.CompanyAddress.PinCode)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.PinCode)} {rule.Error}");
                        break;

                    case "Telephone":
                        RuleFor(x => x.Company.CompanyAddress.AlternatePhone)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.AlternatePhone)} {rule.Error}")
                        .When(x => !string.IsNullOrWhiteSpace(x.Company.CompanyAddress.AlternatePhone));

                        RuleFor(x => x.Company.CompanyAddress.Phone)
                       .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                       .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyAddress.Phone)} {rule.Error}")
                       .When(x => !string.IsNullOrWhiteSpace(x.Company.CompanyAddress.Phone));

                        RuleFor(x => x.Company.CompanyContact.Phone)
                       .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                       .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Phone)} {rule.Error}");
                        break;

                    case "Email":
                        RuleFor(x => x.Company.CompanyContact.Email)
                        .EmailAddress()
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.CompanyContact.Email)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => x.Company.CompanyName)
                        .MustAsync(async (companyName, cancellation) => !await _companyRepository.CompanyExistsAsync(companyName))
                        .WithName("Company Name")
                         .WithMessage($"{rule.Error}");

                        RuleFor(x => x.Company.PanNumber)
                        .MustAsync(async (panNumber, cancellation) => !await _companyRepository.PanNumberExistsAsync(panNumber))
                        .WithName("Pan Number")
                        .WithMessage($"{rule.Error}");
                        break;

                    case "PanFormat":
                        RuleFor(x => x.Company.PanNumber)
                        .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                        .WithMessage($"{nameof(CreateCompanyCommand.Company.PanNumber)} {rule.Error}");
                        break;

                    default:

                        break;
                }
            }
        }
    }
}