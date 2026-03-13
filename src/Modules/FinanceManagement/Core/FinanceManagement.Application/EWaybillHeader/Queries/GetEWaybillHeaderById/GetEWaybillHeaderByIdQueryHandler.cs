using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderById
{
    public class GetEWaybillHeaderByIdQueryHandler : IRequestHandler<GetEWaybillHeaderByIdQuery, EWaybillHeaderDto?>
    {
        private readonly IEWaybillHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetEWaybillHeaderByIdQueryHandler(
            IEWaybillHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<EWaybillHeaderDto?> Handle(GetEWaybillHeaderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<EWaybillHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetEWaybillHeaderByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"EWaybill Header details {dto.Id} was fetched.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
