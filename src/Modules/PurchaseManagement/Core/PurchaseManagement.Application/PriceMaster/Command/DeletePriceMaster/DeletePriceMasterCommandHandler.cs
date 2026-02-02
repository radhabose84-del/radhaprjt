// Core.Application/PriceMaster/Commands/Delete/SoftDeletePriceMasterCommandHandler.cs
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PriceMaster.Commands.Delete
{
    public sealed class DeletePriceMasterCommandHandler
        : IRequestHandler<DeletePriceMasterCommand, bool>
    {
        private readonly IPriceMasterCommandRepository _priceMasterCommandRepository;
        private readonly IPriceMasterQueryRepository _priceMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeletePriceMasterCommandHandler(IPriceMasterCommandRepository priceMasterCommandRepository, IPriceMasterQueryRepository priceMasterQueryRepository, IMediator mediator)
        {
            _priceMasterCommandRepository = priceMasterCommandRepository;
            _priceMasterQueryRepository = priceMasterQueryRepository;
            _mediator = mediator;
        }


        public async Task<bool> Handle(DeletePriceMasterCommand request, CancellationToken ct)
        {
            var existing = await _priceMasterQueryRepository.GetByIdAsync(request.Id,ct);
            if (existing is null)
                throw new ExceptionRules($"Payment term not found for Id={request.Id}.");

            var ok = await _priceMasterCommandRepository.DeleteAsync(request.Id);
            if (!ok) throw new ExceptionRules("Soft delete failed.");

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: existing.Id.ToString(),
                actionName: existing.ItemCode,
                details: $"PaymentTermMaster '{existing.ItemCode}' soft-deleted (Id={request.Id}).",
                module: "PaymentTermMaster"));

            return true;
        }
    }
}
