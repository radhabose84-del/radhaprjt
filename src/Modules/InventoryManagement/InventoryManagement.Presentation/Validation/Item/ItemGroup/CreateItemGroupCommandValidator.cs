using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemGroup
{
    public class CreateItemGroupCommandValidator : AbstractValidator<CreateItemGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemGroupCommandRepository _itemGroupCommandRepository;

        public CreateItemGroupCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemGroupCommandRepository itemGroupCommandRepository)
        {
            var maxLength =
                maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemGroup>("ItemGroupName") ?? 100;

            _itemGroupCommandRepository = itemGroupCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
                throw new ArgumentException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ItemGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemGroupCommand.ItemGroupName)} {rule.Error}");

                        RuleFor(x => x.ItemGroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemGroupCommand.ItemGroupCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ItemGroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateItemGroupCommand.ItemGroupName)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ItemGroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemGroupCommand.ItemGroupCode)} {rule.Error}")
                            .MustAsync(async (_, code, ct) =>
                                !await _itemGroupCommandRepository.ExistsByCodeAsync(code))
                            .WithMessage("Item Group Code already exists.");

                        RuleFor(x => (x.ItemGroupName ?? string.Empty).Trim())
                            .Matches("^[A-Za-z0-9 _-]+$")
                            .WithMessage("Special characters are not allowed only alphanumeric values are allowed")
                            .WithName(nameof(CreateItemGroupCommand.ItemGroupName));

                        RuleFor(x => (x.ItemGroupCode ?? string.Empty).Trim())
                            .Matches("^[A-Za-z0-9_-]+$")
                            .WithMessage("Special characters are not allowed only alphanumeric values are allowed")
                            .WithName(nameof(CreateItemGroupCommand.ItemGroupCode)); 



                        RuleFor(x => x.ItemGroupName)
                            .MustAsync(async (_, name, ct) =>
                                !await _itemGroupCommandRepository.ExistsByNameAsync(name, ct))
                            .WithMessage("Item Group Name already exists.");
                        break;
                }
            }

            RuleFor(x => (x.ItemGroupCode ?? string.Empty).Trim())
                .Matches("^[A-Za-z0-9_-]+$")
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");

            RuleFor(x => (x.ItemGroupName ?? string.Empty).Trim())
                .Matches("^[A-Za-z0-9 _-]+$")
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");
        }
    }
}
