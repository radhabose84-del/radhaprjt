using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQueryHandler : IRequestHandler<GetMiscTypeMasterByIdQuery, MiscTypeMasterDto?>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscTypeMasterByIdQueryHandler(IMiscTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MiscTypeMasterDto?> Handle(GetMiscTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var miscTypeMaster = _mapper.Map<MiscTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMiscTypeMasterByIdQuery",
                actionName: miscTypeMaster.Id.ToString(),
                details: $"MiscTypeMaster details {miscTypeMaster.Id} was fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return miscTypeMaster;
        }
    }
}
