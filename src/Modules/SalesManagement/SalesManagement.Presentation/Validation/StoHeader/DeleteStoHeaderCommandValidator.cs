using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Commands.DeleteStoHeader;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoHeader
{
    public class DeleteStoHeaderCommandValidator : AbstractValidator<DeleteStoHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoHeaderQueryRepository _queryRepository;

        public DeleteStoHeaderCommandValidator(IStoHeaderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteStoHeaderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Stock Transfer Order {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
