using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.FreightMaster.Queries.GetFreightMasterById
{
    public class GetFreightMasterByIdQueryHandler : IRequestHandler<GetFreightMasterByIdQuery, FreightMasterDto?>
    {
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFreightMasterByIdQueryHandler(IFreightMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<FreightMasterDto?> Handle(GetFreightMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetFreightMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"FreightMaster details {result.Id} was fetched.",
                module: "FreightMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
