using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.CreateMovementTypeConfig;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MovementTypeConfig
{
    public class CreateMovementTypeConfigCommandValidator : AbstractValidator<CreateMovementTypeConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMovementTypeConfigQueryRepository _queryRepository;

        public CreateMovementTypeConfigCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMovementTypeConfigQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.MovementTypeConfig>("MovementCode") ?? 4;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.MovementTypeConfig>("MovementDescription") ?? 100;
            var maxLengthAccountModifier = maxLengthProvider.GetMaxLength<Domain.Entities.MovementTypeConfig>("AccountModifier") ?? 50;

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
                        RuleFor(x => x.MovementCode)
                            .NotNull().WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCode)} {rule.Error}");

                        RuleFor(x => x.MovementDescription)
                            .NotNull().WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementDescription)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementDescription)} {rule.Error}");

                        RuleFor(x => x.MovementCategoryId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCategoryId)} {rule.Error}");

                        RuleFor(x => x.FromStockTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateMovementTypeConfigCommand.FromStockTypeId)} {rule.Error}");

                        RuleFor(x => x.ToStockTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateMovementTypeConfigCommand.ToStockTypeId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.MovementCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MovementCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MovementCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.MovementDescription)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementDescription)} {rule.Error} {maxLengthDesc} characters.");

                        RuleFor(x => x.AccountModifier)
                            .MaximumLength(maxLengthAccountModifier)
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.AccountModifier)} {rule.Error} {maxLengthAccountModifier} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountModifier));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.MovementCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.MovementCode));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MovementCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.MovementCategoryId)} {rule.Error}")
                            .When(x => x.MovementCategoryId > 0);

                        RuleFor(x => x.FromStockTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.FromStockTypeId)} {rule.Error}")
                            .When(x => x.FromStockTypeId > 0);

                        RuleFor(x => x.ToStockTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateMovementTypeConfigCommand.ToStockTypeId)} {rule.Error}")
                            .When(x => x.ToStockTypeId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Cross-field: FromStockTypeId must differ from ToStockTypeId
            RuleFor(x => x.ToStockTypeId)
                .Must((cmd, toId) => toId != cmd.FromStockTypeId)
                .WithMessage("FromStockType and ToStockType must be different.")
                .When(x => x.FromStockTypeId > 0 && x.ToStockTypeId > 0);

            // Conditional: AccountModifier required when ValueUpdateFlag is true
            RuleFor(x => x.AccountModifier)
                .NotEmpty()
                .WithMessage("AccountModifier is required when ValueUpdateFlag is enabled.")
                .When(x => x.ValueUpdateFlag);
        }
    }
}
