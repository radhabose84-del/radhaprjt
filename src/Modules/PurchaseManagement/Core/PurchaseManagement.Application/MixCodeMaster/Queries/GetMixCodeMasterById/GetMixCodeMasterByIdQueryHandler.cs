using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterById
{
    public class GetMixCodeMasterByIdQueryHandler : IRequestHandler<GetMixCodeMasterByIdQuery, MixCodeMasterDto?>
    {
        private readonly IMixCodeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetMixCodeMasterByIdQueryHandler(IMixCodeMasterQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<MixCodeMasterDto?> Handle(GetMixCodeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMixCodeMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"MixCodeMaster details {result.Id} was fetched.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
