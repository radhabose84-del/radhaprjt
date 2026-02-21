#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;

namespace SalesManagement.Presentation.Validation.SalesSegment
{
    public class CreateSalesSegmentCommandValidator : AbstractValidator<CreateSalesSegmentCommand>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;

        public CreateSalesSegmentCommandValidator(ISalesSegmentQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // 1. SalesOrganisationId required + exists
            RuleFor(x => x.SalesOrganisationId)
                .GreaterThan(0).WithMessage("Sales Organisation is required.")
                .MustAsync(async (id, ct) => await _queryRepository.SalesOrganisationExistsAsync(id))
                .WithMessage("Sales Organisation does not exist in Sales Organisation Master.");

            // 2. SalesChannelId required + exists
            RuleFor(x => x.SalesChannelId)
                .GreaterThan(0).WithMessage("Sales Channel is required.")
                .MustAsync(async (id, ct) => await _queryRepository.SalesChannelExistsAsync(id))
                .WithMessage("Sales Channel does not exist in Sales Channel Master.");

            // 3. BusinessUnitId required + exists
            RuleFor(x => x.BusinessUnitId)
                .GreaterThan(0).WithMessage("Business Unit is required.")
                .MustAsync(async (id, ct) => await _queryRepository.BusinessUnitExistsAsync(id))
                .WithMessage("Business Unit does not exist in Business Unit Master.");

            // 4. SegmentName required
            RuleFor(x => x.SegmentName)
                .NotEmpty().WithMessage("Segment Name is required.")
                .MaximumLength(200).WithMessage("Segment Name cannot exceed 200 characters.");

            // 5. Composite key uniqueness
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await _queryRepository.CompositeKeyExistsAsync(
                        cmd.SalesOrganisationId,
                        cmd.SalesChannelId,
                        cmd.BusinessUnitId))
                .WithMessage("This combination of Sales Organisation, Sales Channel, and Business Unit already exists.");

            // Note: CurrencyId is optional and validated in handler via ICurrencyLookup
        }
    }
}
