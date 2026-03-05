using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceById
{
    public class GetDispatchAdviceByIdQueryHandler : IRequestHandler<GetDispatchAdviceByIdQuery, DispatchAdviceHeaderDto?>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdviceByIdQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DispatchAdviceHeaderDto?> Handle(GetDispatchAdviceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDispatchAdviceByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Dispatch Advice details {result.Id} was fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
