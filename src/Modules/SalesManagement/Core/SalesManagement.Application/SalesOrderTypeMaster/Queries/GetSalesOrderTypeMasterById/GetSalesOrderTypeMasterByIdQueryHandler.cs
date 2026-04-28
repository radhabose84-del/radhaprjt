using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterById
{
    public class GetSalesOrderTypeMasterByIdQueryHandler
        : IRequestHandler<GetSalesOrderTypeMasterByIdQuery, SalesOrderTypeMasterDto?>
    {
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOrderTypeMasterByIdQueryHandler(
            ISalesOrderTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesOrderTypeMasterDto?> Handle(
            GetSalesOrderTypeMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result is null)
                return null;

            var dto = _mapper.Map<SalesOrderTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesOrderTypeMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"SalesOrderTypeMaster details {dto.Id} was fetched.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
