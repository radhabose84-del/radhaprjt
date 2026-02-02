using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster
{
    public class DeletePaymentTermMasterCommandHandler : IRequestHandler<DeletePaymentTermMasterCommand, bool>
{
    private readonly IPaymentTermMasterCommandRepository _paymentTermMasterCommandRepository;
    private readonly IPaymentTermMasterQueryRepository  _paymentTermMasterQueryRepository;
    private readonly IMediator _mediator;

    public DeletePaymentTermMasterCommandHandler(IPaymentTermMasterCommandRepository commands,  IPaymentTermMasterQueryRepository queries,IMediator mediator    )
    {
        _paymentTermMasterCommandRepository = commands;
        _paymentTermMasterQueryRepository  = queries;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeletePaymentTermMasterCommand request , CancellationToken cancellationToken)
    {
        // Optional: block delete if used in transactions; otherwise proceed.
        var existing = await _paymentTermMasterQueryRepository.GetByIdAsync(request.Id  );
        if (existing is null)
            throw new ExceptionRules($"Payment term not found for Id={request.Id}.");

        var ok = await _paymentTermMasterCommandRepository.DeleteAsync(request.Id );
        if (!ok) throw new ExceptionRules("Soft delete failed.");

        await _mediator.Publish(new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: existing.Code,
            actionName: existing.Description,
            details: $"PaymentTermMaster '{existing.Code}' soft-deleted (Id={request.Id}).",
            module: "PaymentTermMaster"));

        return true;
    }
        
    }
}