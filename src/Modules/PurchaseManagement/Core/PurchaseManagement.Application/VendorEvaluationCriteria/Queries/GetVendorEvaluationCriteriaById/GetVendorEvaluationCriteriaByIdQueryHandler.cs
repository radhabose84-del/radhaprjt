using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaById
{
    public class GetVendorEvaluationCriteriaByIdQueryHandler : IRequestHandler<GetVendorEvaluationCriteriaByIdQuery, VendorEvaluationCriteriaDto?>
    {
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorEvaluationCriteriaByIdQueryHandler(IVendorEvaluationCriteriaQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VendorEvaluationCriteriaDto?> Handle(GetVendorEvaluationCriteriaByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null)
                return null;

            var dto = _mapper.Map<VendorEvaluationCriteriaDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetVendorEvaluationCriteriaByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"VendorEvaluationCriteria details {dto.Id} was fetched.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
