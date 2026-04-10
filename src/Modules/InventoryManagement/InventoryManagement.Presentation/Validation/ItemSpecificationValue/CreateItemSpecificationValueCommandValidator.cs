using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Presentation.Validation.ItemSpecificationValue
{
    public class CreateItemSpecificationValueCommandValidator : AbstractValidator<CreateItemSpecificationValueCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemSpecificationValueQueryRepository _queryRepo;

        public CreateItemSpecificationValueCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IItemSpecificationValueQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthValue = maxLengthProvider.GetMaxLength<DomainEntities.ItemSpecificationValue>("SpecificationValue") ?? 100;

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
                        RuleFor(x => x.SpecificationValue)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationValue)
                            .MaximumLength(maxLengthValue)
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationValue)} {rule.Error} {maxLengthValue} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SpecificationValue)
                            .MustAsync(async (cmd, value, ct) =>
                                !await _queryRepo.AlreadyExistsAsync(cmd.SpecificationMasterId, value!))
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationValue)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationValue) && x.SpecificationMasterId > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SpecificationMasterId)
                            .MustAsync(async (id, ct) => await _queryRepo.SpecificationMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationMasterId)} {rule.Error}")
                            .When(x => x.SpecificationMasterId > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.SpecificationMasterId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateItemSpecificationValueCommand.SpecificationMasterId)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
