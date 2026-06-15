using AutoMapper;
using FinanceManagement.Application.AccountTypeMaster.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Queries.GetAccountTypeMasterById
{
    public class GetAccountTypeMasterByIdQueryHandler : IRequestHandler<GetAccountTypeMasterByIdQuery, AccountTypeMasterDto?>
    {
        private readonly IAccountTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAccountTypeMasterByIdQueryHandler(IAccountTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AccountTypeMasterDto?> Handle(GetAccountTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<AccountTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetAccountTypeMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"AccountTypeMaster details {dto.Id} was fetched.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
