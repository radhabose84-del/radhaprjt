using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesChannel
{
    public class DeleteSalesChannelCommandValidator : AbstractValidator<DeleteSalesChannelCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesChannelQueryRepository _queryRepository;

        public DeleteSalesChannelCommandValidator(ISalesChannelQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesChannelCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Channel {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
