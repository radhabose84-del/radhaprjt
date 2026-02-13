using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using FluentValidation;
using PartyManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PartyManagement.Presentation.Validation.PartyGroup
{
    public class UpdatePartyGroupCommandValidator : AbstractValidator<UpdatePartyGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyGroupCommandRepository _ipartygroupcommandrepository;
        public UpdatePartyGroupCommandValidator(IPartyGroupCommandRepository ipartygroupcommandrepository, MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(UpdatePartyGroupCommand.PartyGroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.PartyGroupName)
                            .MaximumLength(PartyGroupNameMaxLength)
                            .WithMessage($"{nameof(UpdatePartyGroupCommand.PartyGroupName)} {rule.Error} {PartyGroupNameMaxLength}");
                        RuleFor(x => x.Description)
                           .MaximumLength(DescriptionMaxLength)
                           .WithMessage($"{nameof(UpdatePartyGroupCommand.Description)} {rule.Error} {DescriptionMaxLength}");
                       break;

                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.PartyGroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdatePartyGroupCommand.PartyGroupName)} {rule.Error}");
                        break;
                  case "AlreadyExists":
                   RuleFor(x => x)
                    .MustAsync(async (command, cancellation) =>
                    {
                        var groupTypeId = await _ipartygroupcommandrepository.GetGroupTypeIdByIdAsync(command.Id);
                        if (groupTypeId == null)
                            return false;

                        return !await _ipartygroupcommandrepository.ExistsUpdateAsync(command.PartyGroupName!, groupTypeId.Value, command.Id);
                    })
                    .WithMessage("A Party Group with the same name already exists in the same group type.");
                    break;
                  
                }
            }
        }
       
    }
}