using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHeaderById
{
    public class GetVendorEvaluationHeaderByIdQueryHandler : IRequestHandler<GetVendorEvaluationHeaderByIdQuery, VendorEvaluationHeaderDto?>
    {
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorEvaluationHeaderByIdQueryHandler(IVendorEvaluationHeaderQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VendorEvaluationHeaderDto?> Handle(GetVendorEvaluationHeaderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null) return null;

            var dto = _mapper.Map<VendorEvaluationHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetVendorEvaluationHeaderByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"VendorEvaluationHeader details {dto.Id} was fetched.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
