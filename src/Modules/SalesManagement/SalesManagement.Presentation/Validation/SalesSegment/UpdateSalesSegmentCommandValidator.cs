#nullable disable

using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesSegment
{
    public class UpdateSalesSegmentCommandValidator : AbstractValidator<UpdateSalesSegmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;

        public UpdateSalesSegmentCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;

            var maxLengthSegmentName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesSegment>("SegmentName") ?? 200;

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
                        RuleFor(x => x.SegmentName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesSegmentCommand.SegmentName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesSegmentCommand.SegmentName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SegmentName)
                            .MaximumLength(maxLengthSegmentName)
                            .WithMessage($"{nameof(UpdateSalesSegmentCommand.SegmentName)} {rule.Error} {maxLengthSegmentName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Sales Segment Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Segment {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (currencyId, ct) =>
                            {
                                var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId.Value }, ct);
                                return currencies.Any();
                            })
                            .WithMessage($"{nameof(UpdateSalesSegmentCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesSegmentCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
