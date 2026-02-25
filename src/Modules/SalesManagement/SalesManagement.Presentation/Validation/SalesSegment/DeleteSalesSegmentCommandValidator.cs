#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesSegment
{
    public class DeleteSalesSegmentCommandValidator : AbstractValidator<DeleteSalesSegmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesSegmentQueryRepository _queryRepository;

        public DeleteSalesSegmentCommandValidator(ISalesSegmentQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesSegmentCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Segment {rule.Error}");
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
