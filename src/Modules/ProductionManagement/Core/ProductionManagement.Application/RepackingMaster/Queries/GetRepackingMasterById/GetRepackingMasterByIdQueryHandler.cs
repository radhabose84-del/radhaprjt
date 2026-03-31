using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterById
{
    public class GetRepackingMasterByIdQueryHandler
        : IRequestHandler<GetRepackingMasterByIdQuery, RepackingMasterDto?>
    {
        private readonly IRepackingMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingMasterByIdQueryHandler(
            IRepackingMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<RepackingMasterDto?> Handle(
            GetRepackingMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRepackingMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"RepackingMaster details {result.Id} was fetched.",
                module: "RepackingMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
