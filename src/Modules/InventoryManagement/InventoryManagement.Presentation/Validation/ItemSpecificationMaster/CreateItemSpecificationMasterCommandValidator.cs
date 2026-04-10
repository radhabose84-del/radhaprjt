using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Presentation.Validation.ItemSpecificationMaster
{
    public class CreateItemSpecificationMasterCommandValidator : AbstractValidator<CreateItemSpecificationMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemSpecificationMasterQueryRepository _queryRepo;

        public CreateItemSpecificationMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IItemSpecificationMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<DomainEntities.ItemSpecificationMaster>("SpecificationCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<DomainEntities.ItemSpecificationMaster>("SpecificationName") ?? 100;

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
                        RuleFor(x => x.SpecificationCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationCode)} {rule.Error}");

                        RuleFor(x => x.SpecificationName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SpecificationCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SpecificationCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.SpecificationName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SpecificationCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationCode));

                        RuleFor(x => x.SpecificationName)
                            .MustAsync(async (name, ct) => !await _queryRepo.NameAlreadyExistsAsync(name!))
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.SpecificationName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SpecificationName));

                        RuleFor(x => x.Order)
                            .MustAsync(async (order, ct) => !await _queryRepo.OrderAlreadyExistsAsync(order))
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.Order)} {rule.Error}")
                            .When(x => x.Order > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Order)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateItemSpecificationMasterCommand.Order)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
