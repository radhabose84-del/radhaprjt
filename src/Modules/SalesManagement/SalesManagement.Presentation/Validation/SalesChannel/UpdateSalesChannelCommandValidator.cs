#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;

namespace SalesManagement.Presentation.Validation.SalesChannel
{
    public class UpdateSalesChannelCommandValidator : AbstractValidator<UpdateSalesChannelCommand>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;

        public UpdateSalesChannelCommandValidator(ISalesChannelQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.");

            RuleFor(x => x.SalesChannelName)
                .NotEmpty().WithMessage("Sales Channel Name is required.")
                .MaximumLength(100).WithMessage("Sales Channel Name cannot exceed 100 characters.");
        }
    }
}
