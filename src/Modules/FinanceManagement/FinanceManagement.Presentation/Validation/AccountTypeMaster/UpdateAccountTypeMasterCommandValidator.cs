using FinanceManagement.Application.AccountTypeMaster.Commands.UpdateAccountTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountTypeMaster
{
    public class UpdateAccountTypeMasterCommandValidator : AbstractValidator<UpdateAccountTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountTypeMasterQueryRepository _queryRepository;

        public UpdateAccountTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.AccountTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.AccountTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.AccountTypeName)} {rule.Error}");

                        RuleFor(x => x.StartCode)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.StartCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.StartCode)} {rule.Error}");

                        RuleFor(x => x.AccountCodeLength)
                            .InclusiveBetween(3, 20)
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.AccountCodeLength)} must be between 3 and 20.");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.SortOrder)} must be 0 or greater.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.AccountTypeName)
                            .MaximumLength(maxLengthAccountTypeName)
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.AccountTypeName)} {rule.Error} {maxLengthAccountTypeName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Type Master {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.StartCode)
                .Matches("^[1-9]$")
                .WithMessage($"{nameof(UpdateAccountTypeMasterCommand.StartCode)} must be a single digit between 1 and 9.")
                .When(x => !string.IsNullOrWhiteSpace(x.StartCode));
        }
    }
}
