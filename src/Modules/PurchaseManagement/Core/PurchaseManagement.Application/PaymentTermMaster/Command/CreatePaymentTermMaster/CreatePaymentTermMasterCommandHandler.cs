using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster
{
    public class CreatePaymentTermMasterCommandHandler : IRequestHandler<CreatePaymentTermMasterCommand, int>
    {
        private readonly IPaymentTermMasterCommandRepository _ipaymentTermMasterCommandRepository;
        private readonly IPaymentTermMasterQueryRepository _ipaymentTermMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreatePaymentTermMasterCommandHandler(IPaymentTermMasterCommandRepository ipaymentTermMasterCommandRepository, IPaymentTermMasterQueryRepository ipaymentTermMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipaymentTermMasterCommandRepository = ipaymentTermMasterCommandRepository;
            _ipaymentTermMasterQueryRepository = ipaymentTermMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

          public async Task<int> Handle(CreatePaymentTermMasterCommand request, CancellationToken cancellationToken)
        {
           
            // Map command -> domain entity (includes child rows)
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PaymentTermMaster>(request);

            // Save (EF)
            var newId = await _ipaymentTermMasterCommandRepository.CreateAsync(entity, cancellationToken);
            if (newId <= 0) throw new ExceptionRules("Failed to create Payment Term.");

            // (Optional) read back if you need to return a DTO instead of Id:
            // var dto = await _queries.GetByIdAsync(newId) ?? throw new ExceptionRules("Created Payment Term not found.");

            // Audit event
            var evt = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.Code,
                actionName: entity.Description,
                details: $"PaymentTermMaster '{entity.Code}' created (Id={newId}).",
                module: "PaymentTermMaster"
            );
            await _mediator.Publish(evt, cancellationToken);

            return newId;
        }
    }
}