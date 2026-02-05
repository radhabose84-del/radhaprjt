using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using FluentValidation;
using PartyManagement.API.Validation.Common;

namespace PartyManagement.API.Validation.PartyGroup
{
    public class DeletePartyGroupCommandValidator : AbstractValidator<DeletePartyGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyGroupQueryRepository _ipartyGroupQueryRepository;

        public DeletePartyGroupCommandValidator(IPartyGroupQueryRepository ipartyGroupQueryRepository)
        {
            _validationRules = new List<ValidationRule>();
            _ipartyGroupQueryRepository = ipartyGroupQueryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
                foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeletePartyGroupCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _ipartyGroupQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id not found")
                            .WithMessage($"{rule.Error}");
                            break;
                    default:
                        break;
                }
            }

        }
    }
}