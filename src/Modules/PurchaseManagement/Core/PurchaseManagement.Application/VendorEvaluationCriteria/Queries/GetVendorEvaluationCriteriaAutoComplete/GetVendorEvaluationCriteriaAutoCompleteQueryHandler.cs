using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaAutoComplete
{
    public class GetVendorEvaluationCriteriaAutoCompleteQueryHandler : IRequestHandler<GetVendorEvaluationCriteriaAutoCompleteQuery, IReadOnlyList<VendorEvaluationCriteriaLookupDto>>
    {
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorEvaluationCriteriaAutoCompleteQueryHandler(IVendorEvaluationCriteriaQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<VendorEvaluationCriteriaLookupDto>> Handle(GetVendorEvaluationCriteriaAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<VendorEvaluationCriteriaLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetVendorEvaluationCriteriaAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "VendorEvaluationCriteria details was fetched.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
