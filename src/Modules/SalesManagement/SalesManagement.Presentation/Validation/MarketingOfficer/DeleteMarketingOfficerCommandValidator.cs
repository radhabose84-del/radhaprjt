using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.DeleteMarketingOfficer;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MarketingOfficer
{
    public class DeleteMarketingOfficerCommandValidator : AbstractValidator<DeleteMarketingOfficerCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMarketingOfficerQueryRepository _queryRepository;

        public DeleteMarketingOfficerCommandValidator(IMarketingOfficerQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteMarketingOfficerCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Marketing Officer {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
