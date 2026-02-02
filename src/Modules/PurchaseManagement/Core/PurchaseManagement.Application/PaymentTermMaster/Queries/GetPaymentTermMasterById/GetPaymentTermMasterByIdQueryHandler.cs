using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermMasterById
{
    public class GetPaymentTermMasterByIdQueryHandler : IRequestHandler<GetPaymentTermMasterByIdQuery, PaymentTermMasterDto>
    {

        private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPaymentTermMasterByIdQueryHandler(IPaymentTermMasterQueryRepository paymentTermMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _paymentTermMasterQueryRepository = paymentTermMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
            
           
        public async Task<PaymentTermMasterDto> Handle(  GetPaymentTermMasterByIdQuery request,   CancellationToken cancellationToken)
        {
            var result = await _paymentTermMasterQueryRepository.GetByIdAsync(request.Id);

      

            var dto = _mapper.Map<PaymentTermMasterDto>(result);

            // Domain Event (audit)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: string.Empty,
                actionName: string.Empty,
                details: $"PaymentTermMaster details {dto?.Id} was fetched.",
                module: "PaymentTermMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto!;
        }


    }
}