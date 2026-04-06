using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterById
{
    public class GetDiscountMasterByIdQueryHandler : IRequestHandler<GetDiscountMasterByIdQuery, DiscountMasterDto?>
    {
        private readonly IDiscountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDiscountMasterByIdQueryHandler(IDiscountMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DiscountMasterDto?> Handle(GetDiscountMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDiscountMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"DiscountMaster details {result.Id} was fetched.",
                module: "DiscountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
