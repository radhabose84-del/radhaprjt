using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.AccountTypeMaster.Commands.CreateAccountTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountTypeMaster
{
    public class CreateAccountTypeMasterCommandValidator : AbstractValidator<CreateAccountTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountTypeMasterQueryRepository _queryRepository;
        private readonly ICompanyLookup _companyLookup;

        public CreateAccountTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountTypeMasterQueryRepository queryRepository,
            ICompanyLookup companyLookup)
        {
            _queryRepository = queryRepository;
            _companyLookup = companyLookup;

            var maxLengthAccountTypeName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.AccountTypeMaster>("AccountTypeName") ?? 50;

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
                        RuleFor(x => x.CompanyId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.CompanyId)} {rule.Error}");

                        RuleFor(x => x.AccountTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.AccountTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.AccountTypeName)} {rule.Error}");

                        RuleFor(x => x.StartCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.StartCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.StartCode)} {rule.Error}");

                        RuleFor(x => x.AccountCodeLength)
                            .InclusiveBetween(3, 20)
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.AccountCodeLength)} must be between 3 and 20.");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.SortOrder)} must be 0 or greater.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.AccountTypeName)
                            .MaximumLength(maxLengthAccountTypeName)
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.AccountTypeName)} {rule.Error} {maxLengthAccountTypeName} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CompanyId)
                            .MustAsync(async (companyId, ct) =>
                            {
                                var companies = await _companyLookup.GetAllCompanyAsync();
                                return companies.Any(c => c.CompanyId == companyId);
                            })
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.CompanyId)} {rule.Error}")
                            .When(x => x.CompanyId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.AccountTypeName)
                            .MustAsync(async (command, accountTypeName, ct) =>
                                !await _queryRepository.AlreadyExistsByNameAsync(accountTypeName!, command.CompanyId))
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.AccountTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountTypeName) && x.CompanyId > 0);

                        RuleFor(x => x.StartCode)
                            .MustAsync(async (command, startCode, ct) =>
                                !await _queryRepository.AlreadyExistsByStartCodeAsync(startCode!, command.CompanyId))
                            .WithMessage($"{nameof(CreateAccountTypeMasterCommand.StartCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.StartCode) && x.CompanyId > 0);
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.StartCode)
                .Matches("^[1-9]$")
                .WithMessage($"{nameof(CreateAccountTypeMasterCommand.StartCode)} must be a single digit between 1 and 9.")
                .When(x => !string.IsNullOrWhiteSpace(x.StartCode));
        }
    }
}
