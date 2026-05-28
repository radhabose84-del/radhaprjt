using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeAutoComplete
{
    public class GetVendorRatingGradeAutoCompleteQueryHandler : IRequestHandler<GetVendorRatingGradeAutoCompleteQuery, IReadOnlyList<VendorRatingGradeLookupDto>>
    {
        private readonly IVendorRatingGradeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVendorRatingGradeAutoCompleteQueryHandler(IVendorRatingGradeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<VendorRatingGradeLookupDto>> Handle(GetVendorRatingGradeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<VendorRatingGradeLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetVendorRatingGradeAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "VendorRatingGrade details was fetched.",
                module: "VendorRatingGrade"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
