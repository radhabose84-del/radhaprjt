using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterById
{
    public class GetTncTemplateByIdQueryHandler : IRequestHandler<GetTncTemplateByIdQuery, TncTemplateMasterDto?>
    {

         private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTncTemplateByIdQueryHandler( ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public  async Task<TncTemplateMasterDto?> Handle(GetTncTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _tnCTemplateMasterQueryRepository.GetByIdAsync(request.Id);

      

            var dto = _mapper.Map<TncTemplateMasterDto>(result);

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