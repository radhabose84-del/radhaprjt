using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.OfficerAgent
{
    public class DeleteOfficerAgentCommandValidator : AbstractValidator<DeleteOfficerAgentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public DeleteOfficerAgentCommandValidator(
            IOfficerAgentQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

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

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) =>
                            {
                                if (!_accessFilter.IsMarketingOfficer())
                                    return true;
                                var record = await _queryRepository.GetByIdAsync(id);
                                return record != null;
                            })
                            .WithMessage("You are not authorized to delete this record.")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
