using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TransactionTypeMaster
{
    public class CreateTransactionTypeMasterCommandValidator : AbstractValidator<CreateTransactionTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;

        public CreateTransactionTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITransactionTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthTypeName  = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.TransactionTypeMaster>("TypeName")  ?? 100;
            var maxLengthShortName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.TransactionTypeMaster>("ShortName") ?? 50;
            var maxLengthDesc      = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.TransactionTypeMaster>("Description") ?? 255;

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
                        RuleFor(x => x.TypeName)
                            .NotNull().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.TypeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.TypeName)} {rule.Error}");

                        RuleFor(x => x.ShortName)
                            .NotNull().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ShortName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ShortName)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotEmpty().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.ModuleId)
                            .NotEmpty().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ModuleId)} {rule.Error}");

                        RuleFor(x => x.MenuId)
                            .NotEmpty().WithMessage($"{nameof(CreateTransactionTypeMasterCommand.MenuId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(maxLengthTypeName)
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.TypeName)} {rule.Error} {maxLengthTypeName} characters.");

                        RuleFor(x => x.ShortName)
                            .MaximumLength(maxLengthShortName)
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ShortName)} {rule.Error} {maxLengthShortName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TypeName)
                            .MustAsync(async (typeName, ct) =>
                                !await _queryRepository.TypeNameExistsAsync(typeName!))
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.TypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TypeName));

                        RuleFor(x => x.ShortName)
                            .MustAsync(async (shortName, ct) =>
                                !await _queryRepository.ShortNameExistsAsync(shortName!))
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ShortName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.UnitId)
                            .MustAsync(async (unitId, ct) =>
                                await _queryRepository.UnitExistsAsync(unitId))
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);

                        RuleFor(x => x.ModuleId)
                            .MustAsync(async (moduleId, ct) =>
                                await _queryRepository.ModuleExistsAsync(moduleId))
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.ModuleId)} {rule.Error}")
                            .When(x => x.ModuleId > 0);

                        RuleFor(x => x.MenuId)
                            .MustAsync(async (menuId, ct) =>
                                await _queryRepository.MenuExistsAsync(menuId))
                            .WithMessage($"{nameof(CreateTransactionTypeMasterCommand.MenuId)} {rule.Error}")
                            .When(x => x.MenuId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
