using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class UpdateAccountGroupCommandValidator : AbstractValidator<UpdateAccountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _queryRepository;

        public UpdateAccountGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.AccountGroup>("GroupName") ?? 150;

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
                        RuleFor(x => x.GroupName)
                            .NotNull().WithMessage($"{nameof(UpdateAccountGroupCommand.GroupName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateAccountGroupCommand.GroupName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateAccountGroupCommand.GroupName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Group {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAccountGroupCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Note: the Level 1 statutory head is the immutable AccountTypeId FK set at create
            // time, so no GroupName whitelist is needed here — rename is free text.
        }
    }
}
