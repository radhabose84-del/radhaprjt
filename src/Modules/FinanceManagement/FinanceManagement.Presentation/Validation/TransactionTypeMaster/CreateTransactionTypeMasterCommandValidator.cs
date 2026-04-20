using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TransactionTypeMaster
{
    public class CreateTransactionTypeMasterCommandValidator : AbstractValidator<CreateTransactionTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateTransactionTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITransactionTypeMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

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
                        RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
                        {
                            var unitId = _ipAddressService.GetUnitId() ?? 0;
                            if (unitId <= 0) return;

                            if (!string.IsNullOrWhiteSpace(cmd.TypeName))
                            {
                                var typeNameExists = await _queryRepository.TypeNameExistsAsync(cmd.TypeName!, unitId);
                                if (typeNameExists)
                                    context.AddFailure("TypeName", $"Transaction Type '{cmd.TypeName}' already exists. Please use a different name.");
                            }

                            if (!string.IsNullOrWhiteSpace(cmd.ShortName))
                            {
                                var shortNameExists = await _queryRepository.ShortNameExistsAsync(cmd.ShortName!, unitId);
                                if (shortNameExists)
                                    context.AddFailure("ShortName", $"Transaction Type ShortName '{cmd.ShortName}' already exists. Please use a different name.");
                            }
                        });
                        break;

                    case "FKColumnDelete":
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
