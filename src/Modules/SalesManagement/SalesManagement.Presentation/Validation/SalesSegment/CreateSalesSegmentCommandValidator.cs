
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesSegment
{
    public class CreateSalesSegmentCommandValidator : AbstractValidator<CreateSalesSegmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;

        public CreateSalesSegmentCommandValidator(
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
                        // 1. SalesOrganisationId required
                        RuleFor(x => x.SalesOrganisationId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesOrganisationId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesOrganisationId)} {rule.Error}");

                        // 2. SalesChannelId required
                        RuleFor(x => x.SalesChannelId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesChannelId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesChannelId)} {rule.Error}");

                        // 3. BusinessUnitId required
                        RuleFor(x => x.BusinessUnitId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.BusinessUnitId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.BusinessUnitId)} {rule.Error}");

                        // 4. SegmentName required
                        RuleFor(x => x.SegmentName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SegmentName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SegmentName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SegmentName)
                            .MaximumLength(maxLengthSegmentName)
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SegmentName)} {rule.Error} {maxLengthSegmentName} characters.");
                        break;

                    case "FKColumnDelete":
                        // SalesOrganisationId FK exists
                        RuleFor(x => x.SalesOrganisationId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.SalesOrganisationExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesOrganisationId)} {rule.Error}");

                        // SalesChannelId FK exists
                        RuleFor(x => x.SalesChannelId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.SalesChannelExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.SalesChannelId)} {rule.Error}");

                        // BusinessUnitId FK exists
                        RuleFor(x => x.BusinessUnitId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.BusinessUnitExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.BusinessUnitId)} {rule.Error}");

                        // CurrencyId FK exists (optional cross-module)
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (currencyId, ct) =>
                            {
                                var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId!.Value }, ct);
                                return currencies.Any();
                            })
                            .WithMessage($"{nameof(CreateSalesSegmentCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);
                        break;

                    case "AlreadyExists":
                        // Composite key uniqueness
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.CompositeKeyExistsAsync(
                                    cmd.SalesOrganisationId,
                                    cmd.SalesChannelId,
                                    cmd.BusinessUnitId))
                            .WithMessage($"This combination of Sales Organisation, Sales Channel, and Business Unit {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
