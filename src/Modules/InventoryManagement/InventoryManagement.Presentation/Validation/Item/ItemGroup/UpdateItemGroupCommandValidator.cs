#nullable disable
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemGroup
{
    public class UpdateItemGroupCommandValidator : AbstractValidator<UpdateItemGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemGroupCommandRepository _itemGroupCommandRepository;
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;

        public UpdateItemGroupCommandValidator(
            IMaxLengthProvider maxLengthProvider, // ✅ changed
            IItemGroupCommandRepository itemGroupCommandRepository,
            IItemGroupQueryRepository itemGroupQueryRepository)
        {
            _itemGroupCommandRepository = itemGroupCommandRepository;
            _itemGroupQueryRepository = itemGroupQueryRepository;

            var maxLength = maxLengthProvider
                .GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemGroup>("ItemGroupName") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ItemGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemGroupCommand.ItemGroupName)} {rule.Error}");

                        RuleFor(x => x.ItemGroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemGroupCommand.ItemGroupCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ItemGroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateItemGroupCommand.ItemGroupName)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ItemGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemGroupCommand.ItemGroupName)} {rule.Error}")
                            .MustAsync(async (cmd, name, ct) =>
                                !await _itemGroupCommandRepository.IsNameDuplicateAsync(name, cmd.Id))
                            .WithMessage("A Group Name already exists in this Group.");
                        
                        RuleFor(x => (x.ItemGroupName ?? string.Empty).Trim())
                            .Matches("^[A-Za-z0-9 _-]+$")
                            .WithMessage("Special characters are not allowed only alphanumeric values are allowed")
                            .WithName(nameof(UpdateItemGroupCommand.ItemGroupName));

                        RuleFor(x => (x.ItemGroupCode ?? string.Empty).Trim())
                            .Matches("^[A-Za-z0-9_-]+$")
                            .WithMessage("Special characters are not allowed only alphanumeric values are allowed")
                            .WithName(nameof(UpdateItemGroupCommand.ItemGroupCode)); 

                        RuleFor(x => x.ItemGroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemGroupCommand.ItemGroupCode)} {rule.Error}")
                            .MustAsync(async (cmd, code, ct) =>
                                !await _itemGroupCommandRepository.IsCodeDuplicateAsync(code, cmd.Id))
                            .WithMessage("A Group Code already exists in this Group.");

                        RuleFor(x => x.ItemGroupName)
                            .MustAsync(async (cmd, name, ct) =>
                                !await _itemGroupCommandRepository.ExistsByNameAsync(name, ct))
                            .WithMessage("Item Group Name already exists.");
                        break;

                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => (await _itemGroupQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                        break;
                }
            }

            // Character validation
            RuleFor(x => (x.ItemGroupCode ?? string.Empty).Trim())
                .Matches("^[A-Za-z0-9_-]+$")
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");

            RuleFor(x => (x.ItemGroupName ?? string.Empty).Trim())
                .Matches("^[A-Za-z0-9 _-]+$")
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");
        }
    }
}
