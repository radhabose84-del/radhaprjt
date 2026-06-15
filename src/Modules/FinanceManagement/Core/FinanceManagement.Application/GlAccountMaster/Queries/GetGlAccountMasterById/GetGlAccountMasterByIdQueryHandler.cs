using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterById
{
    public class GetGlAccountMasterByIdQueryHandler : IRequestHandler<GetGlAccountMasterByIdQuery, GlAccountMasterDto?>
    {
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGlAccountMasterByIdQueryHandler(IGlAccountMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GlAccountMasterDto?> Handle(GetGlAccountMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<GlAccountMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetGlAccountMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"GlAccountMaster details {dto.Id} was fetched.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
