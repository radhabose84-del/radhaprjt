#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;

namespace SalesManagement.Presentation.Validation.SalesChannel
{
    public class CreateSalesChannelCommandValidator : AbstractValidator<CreateSalesChannelCommand>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;

        public CreateSalesChannelCommandValidator(ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.SalesChannelCode)
                .NotEmpty().WithMessage("Sales Channel Code is required.")
                .MaximumLength(20).WithMessage("Sales Channel Code cannot exceed 20 characters.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("Sales Channel Code must be alphanumeric only.");

            RuleFor(x => x.SalesChannelCode)
                .MustAsync(async (code, cancellation) =>
                    !await _queryRepository.AlreadyExistsAsync(code))
                .WithMessage("Sales Channel Code already exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.SalesChannelCode));

            RuleFor(x => x.SalesChannelName)
                .NotEmpty().WithMessage("Sales Channel Name is required.")
                .MaximumLength(100).WithMessage("Sales Channel Name cannot exceed 100 characters.");
        }
    }
}
