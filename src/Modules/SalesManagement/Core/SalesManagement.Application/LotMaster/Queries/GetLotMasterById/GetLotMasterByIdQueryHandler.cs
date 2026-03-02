using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ILotMaster;
using SalesManagement.Application.LotMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.LotMaster.Queries.GetLotMasterById
{
    public class GetLotMasterByIdQueryHandler : IRequestHandler<GetLotMasterByIdQuery, LotMasterDto?>
    {
        private readonly ILotMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetLotMasterByIdQueryHandler(
            ILotMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<LotMasterDto?> Handle(GetLotMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetLotMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"LotMaster details {result.Id} was fetched.",
                module: "LotMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
