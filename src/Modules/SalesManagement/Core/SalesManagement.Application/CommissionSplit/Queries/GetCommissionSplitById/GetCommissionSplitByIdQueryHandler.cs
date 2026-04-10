using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitById
{
    public class GetCommissionSplitByIdQueryHandler : IRequestHandler<GetCommissionSplitByIdQuery, CommissionSplitDto?>
    {
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCommissionSplitByIdQueryHandler(ICommissionSplitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CommissionSplitDto?> Handle(GetCommissionSplitByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCommissionSplitByIdQuery",
                actionName: result.Id.ToString(),
                details: $"CommissionSplit details {result.Id} was fetched.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
