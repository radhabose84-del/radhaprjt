using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace PartyManagement.Presentation.Validation.PartyMaster
{
    public class DeletePartyMasterCommandValidator : AbstractValidator<DeletePartyMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPartyMasterQueryRepository _ipartymasterQueryRepository;

        public DeletePartyMasterCommandValidator(IPartyMasterQueryRepository ipartymasterQueryRepository)
        {
            _validationRules = new List<ValidationRule>();
            _ipartymasterQueryRepository = ipartymasterQueryRepository;
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
                            .WithMessage($"{nameof(DeletePartyMasterCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _ipartymasterQueryRepository.GetByIdPartyMasterAsync(id)) != null) 
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