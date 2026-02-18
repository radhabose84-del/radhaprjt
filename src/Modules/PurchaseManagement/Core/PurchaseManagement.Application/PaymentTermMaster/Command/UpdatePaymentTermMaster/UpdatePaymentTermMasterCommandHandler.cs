using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Domain.Events;
using MediatR;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster
{
    public class UpdatePaymentTermMasterCommandHandler : IRequestHandler<UpdatePaymentTermMasterCommand, bool>
    {

        private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;
        private readonly IPaymentTermMasterCommandRepository _ipaymentTermMasterCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdatePaymentTermMasterCommandHandler(IPaymentTermMasterQueryRepository queries, IPaymentTermMasterCommandRepository commands, IMapper mapper, IMediator mediator)
        {
            _paymentTermMasterQueryRepository = queries;
            _ipaymentTermMasterCommandRepository = commands;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdatePaymentTermMasterCommand request , CancellationToken cancellationToken)
        {
             var existing = await _paymentTermMasterQueryRepository.GetByIdAsync(request.Id);
            if (existing is null)
                throw new ExceptionRules($"Payment term not found for Id={request.Id}.");

            var incoming = new PurchaseManagement.Domain.Entities.PaymentTermMaster
            {
                Id = request.Id,
                Code = request.Code.Trim(),
                Description = request.Description,
                BaselineTypeId = request.BaselineTypeId,
                CreditDays = request.CreditDays,
                AdvancePercent = request.AdvancePercent,
                DiscountPercent = request.DiscountPercent,
                DiscountDays = request.DiscountDays,
                GraceDays = request.GraceDays,
                ApplicableForPortal = request.ApplicableForPortal,
                IsActive = request.IsActive ? Status.Active : Status.Inactive
                
            };

            var newInstallments = request.Installments?
                .OrderBy(i => i.SeqNo)
                .Select(i => new PurchaseManagement.Domain.Entities.PaymentTermInstallment
                {
                    PaymentTermId = request.Id,
                    SeqNo = i.SeqNo,
                    Percent = i.Percent,
                    DueDays = i.DueDays
                    
                })
                .ToList();

            var ok = await _ipaymentTermMasterCommandRepository.UpdateAsync(incoming, newInstallments);
            if (!ok) throw new ExceptionRules("Update failed.");

            var evt = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: incoming.Code,
                actionName: incoming.Description,
                details: $"PaymentTermMaster '{incoming.Code}' updated (Id={incoming.Id}).",
                module: "PaymentTermMaster"
            );
            await _mediator.Publish(evt);

            return true;
        }
    }
}