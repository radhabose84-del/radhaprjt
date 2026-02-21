#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;

namespace SalesManagement.Presentation.Validation.SalesSegment
{
    public class UpdateSalesSegmentCommandValidator : AbstractValidator<UpdateSalesSegmentCommand>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;

        public UpdateSalesSegmentCommandValidator(ISalesSegmentQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            // 1. Id must exist
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Sales Segment Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Sales Segment not found.");

            // 2. SegmentName required
            RuleFor(x => x.SegmentName)
                .NotEmpty().WithMessage("Segment Name is required.")
                .MaximumLength(200).WithMessage("Segment Name cannot exceed 200 characters.");

            // 3. IsActive valid values
            RuleFor(x => x.IsActive)
                .Must(x => x == 0 || x == 1)
                .WithMessage("IsActive must be 0 (Inactive) or 1 (Active).");

            // Note: Composite key fields (SalesOrganisationId, SalesChannelId, BusinessUnitId) are IMMUTABLE
            // They cannot be changed after creation - not included in update command

            // Note: CurrencyId is optional and validated in handler via ICurrencyLookup
        }
    }
}
