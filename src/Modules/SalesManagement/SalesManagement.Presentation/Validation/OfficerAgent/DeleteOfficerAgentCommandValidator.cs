using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.OfficerAgent
{
    public class DeleteOfficerAgentCommandValidator : AbstractValidator<DeleteOfficerAgentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IOfficerAgentQueryRepository _queryRepository;

        public DeleteOfficerAgentCommandValidator(IOfficerAgentQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteOfficerAgentCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"OfficerAgent {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
