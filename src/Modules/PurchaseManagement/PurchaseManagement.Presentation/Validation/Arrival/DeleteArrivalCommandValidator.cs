using FluentValidation;
using PurchaseManagement.Application.Arrival.Commands.DeleteArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.Presentation.Validation.Arrival
{
    public class DeleteArrivalCommandValidator : AbstractValidator<DeleteArrivalCommand>
    {
        public DeleteArrivalCommandValidator(IArrivalQueryRepository queryRepository)
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.")
                .MustAsync(async (id, ct) => !await queryRepository.NotFoundAsync(id))
                .WithMessage("Arrival not found.");
        }
    }
}
