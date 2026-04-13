using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.TransactionTypeMaster
{
    public class UpdateTransactionTypeMasterCommandValidator : AbstractValidator<UpdateTransactionTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;
        private readonly IUnitLookup _unitLookup;

        public UpdateTransactionTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ITransactionTypeMasterQueryRepository queryRepository,
            IUnitLookup unitLookup)
        {
            _queryRepository = queryRepository;
            _unitLookup = unitLookup;

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
                            .NotNull().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.TypeName)} {rule.Error}");

                        RuleFor(x => x.ShortName)
                            .NotNull().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ShortName)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.ModuleId)
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.ModuleId)} {rule.Error}");

                        RuleFor(x => x.MenuId)
                            .NotEmpty().WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.MenuId)} {rule.Error}");
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
                        RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
                        {
                            IReadOnlyList<Contracts.Dtos.Lookups.Users.UnitLookupDto>? units = null;

                            if (!string.IsNullOrWhiteSpace(cmd.TypeName) && cmd.UnitId > 0)
                            {
                                var typeNameExists = await _queryRepository.TypeNameExistsAsync(cmd.TypeName!, cmd.UnitId, cmd.Id);
                                if (typeNameExists)
                                {
                                    units ??= await _unitLookup.GetAllUnitAsync();
                                    var unitName = units.FirstOrDefault(u => u.UnitId == cmd.UnitId)?.UnitName ?? cmd.UnitId.ToString();
                                    context.AddFailure("TypeName", $"Transaction Type '{cmd.TypeName}' already exists for Unit '{unitName}'. Please use a different name.");
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(cmd.ShortName) && cmd.UnitId > 0)
                            {
                                var shortNameExists = await _queryRepository.ShortNameExistsAsync(cmd.ShortName!, cmd.UnitId, cmd.Id);
                                if (shortNameExists)
                                {
                                    units ??= await _unitLookup.GetAllUnitAsync();
                                    var unitName = units.FirstOrDefault(u => u.UnitId == cmd.UnitId)?.UnitName ?? cmd.UnitId.ToString();
                                    context.AddFailure("ShortName", $"Transaction Type ShortName '{cmd.ShortName}' already exists for Unit '{unitName}'. Please use a different name.");
                                }
                            }
                        });
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

                        RuleFor(x => x.MenuId)
                            .MustAsync(async (menuId, ct) =>
                                await _queryRepository.MenuExistsAsync(menuId))
                            .WithMessage($"{nameof(UpdateTransactionTypeMasterCommand.MenuId)} {rule.Error}")
                            .When(x => x.MenuId > 0);
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
