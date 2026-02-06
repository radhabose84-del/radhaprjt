using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using FluentValidation;
using PartyManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace PartyManagement.API.Validation.PartyGroup
{
    public class CreatePartyGroupCommandValidator : AbstractValidator<CreatePartyGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyGroupCommandRepository _ipartygroupcommandrepository;

        public CreatePartyGroupCommandValidator(IPartyGroupCommandRepository ipartygroupcommandrepository, MaxLengthProvider maxLengthProvider)
        {
            _ipartygroupcommandrepository = ipartygroupcommandrepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            var PartyGroupNameMaxLength = maxLengthProvider.GetMaxLength<PartyManagement.Domain.Entities.PartyGroup>("PartyGroupName") ?? 100;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<PartyManagement.Domain.Entities.PartyGroup>("Description") ?? 250;
             if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.PartyGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePartyGroupCommand.PartyGroupName)} {rule.Error}");
                        RuleFor(x => x.GroupTypeId)
                          .NotEmpty()
                          .WithMessage($"{nameof(CreatePartyGroupCommand.GroupTypeId)} {rule.Error} {0}");
                        // RuleFor(x => x.IsGroup)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreatePartyGroupCommand.IsGroup)} {rule.Error} {0}");
                        break;

                    case "MinLength":
                        RuleFor(x => x.GroupTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePartyGroupCommand.GroupTypeId)} {rule.Error} {0}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.PartyGroupName)
                            .MaximumLength(PartyGroupNameMaxLength)
                            .WithMessage($"{nameof(CreatePartyGroupCommand.PartyGroupName)} {rule.Error} {PartyGroupNameMaxLength}");
                        RuleFor(x => x.Description)
                           .MaximumLength(DescriptionMaxLength)
                           .WithMessage($"{nameof(CreatePartyGroupCommand.Description)} {rule.Error} {DescriptionMaxLength}");
                       break;

                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.PartyGroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreatePartyGroupCommand.PartyGroupName)} {rule.Error}");
                        break;
                  case "AlreadyExists":
                    RuleFor(x => new { x.PartyGroupName, x.GroupTypeId })
                    .MustAsync(async (input, cancellation) =>
                        !await _ipartygroupcommandrepository.ExistsAsync(input.PartyGroupName, input.GroupTypeId))
                    .WithName("PartyGroupName")
                    .WithMessage("A Party Group with the same name and group type already exists.");
                    break;
                  
                }
            }

        }
    }
}