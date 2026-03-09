using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.TransactionTypeMaster
{
    public class UpdateTransactionTypeMasterCommandValidator : AbstractValidator<UpdateTransactionTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;

        public UpdateTransactionTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITransactionTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthTypeName  = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.TransactionTypeMaster>("TypeName")  ?? 100;
            var maxLengthShortName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.TransactionTypeMaster>("ShortName") ?? 50;
            var maxLengthDesc      = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.TransactionTypeMaster>("Description") ?? 255;

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
                            .NotNull().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error}");

                        RuleFor(x => x.ShortName)
                            .NotNull().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.ModuleId)
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ModuleId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(maxLengthTypeName)
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error} {maxLengthTypeName} characters.");

                        RuleFor(x => x.ShortName)
                            .MaximumLength(maxLengthShortName)
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error} {maxLengthShortName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Transaction Type Master {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.TypeName)
                            .MustAsync(async (command, typeName, ct) =>
                                !await _queryRepository.TypeNameExistsAsync(typeName!, command.Id))
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.TypeName));

                        RuleFor(x => x.ShortName)
                            .MustAsync(async (command, shortName, ct) =>
                                !await _queryRepository.ShortNameExistsAsync(shortName!, command.Id))
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.UnitId)
                            .MustAsync(async (unitId, ct) =>
                                await _queryRepository.UnitExistsAsync(unitId))
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.UnitId)} {rule.Error}")
                            .When(x => x.UnitId > 0);

                        RuleFor(x => x.ModuleId)
                            .MustAsync(async (moduleId, ct) =>
                                await _queryRepository.ModuleExistsAsync(moduleId))
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ModuleId)} {rule.Error}")
                            .When(x => x.ModuleId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
