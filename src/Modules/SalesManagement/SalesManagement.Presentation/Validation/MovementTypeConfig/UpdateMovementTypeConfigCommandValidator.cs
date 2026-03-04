using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.UpdateMovementTypeConfig;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MovementTypeConfig
{
    public class UpdateMovementTypeConfigCommandValidator : AbstractValidator<UpdateMovementTypeConfigCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMovementTypeConfigQueryRepository _queryRepository;

        public UpdateMovementTypeConfigCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMovementTypeConfigQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.MovementDescription)
                            .NotNull().WithMessage($"{nameof(UpdateMovementTypeConfigCommand.MovementDescription)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateMovementTypeConfigCommand.MovementDescription)} {rule.Error}");

                        RuleFor(x => x.MovementCategoryId)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateMovementTypeConfigCommand.MovementCategoryId)} {rule.Error}");

                        RuleFor(x => x.FromStockTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateMovementTypeConfigCommand.FromStockTypeId)} {rule.Error}");

                        RuleFor(x => x.ToStockTypeId)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateMovementTypeConfigCommand.ToStockTypeId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.MovementDescription)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.MovementDescription)} {rule.Error} {maxLengthDesc} characters.");

                        RuleFor(x => x.AccountModifier)
                            .MaximumLength(maxLengthAccountModifier)
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.AccountModifier)} {rule.Error} {maxLengthAccountModifier} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AccountModifier));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Movement Type Config ID is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Movement Type Config {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MovementCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.MovementCategoryId)} {rule.Error}")
                            .When(x => x.MovementCategoryId > 0);

                        RuleFor(x => x.FromStockTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.FromStockTypeId)} {rule.Error}")
                            .When(x => x.FromStockTypeId > 0);

                        RuleFor(x => x.ToStockTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.ToStockTypeId)} {rule.Error}")
                            .When(x => x.ToStockTypeId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMovementTypeConfigCommand.IsActive)} {rule.Error}");
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
