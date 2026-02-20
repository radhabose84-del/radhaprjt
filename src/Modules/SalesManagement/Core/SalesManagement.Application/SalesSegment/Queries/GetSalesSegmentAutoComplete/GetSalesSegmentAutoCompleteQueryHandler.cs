#nullable disable

using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentAutoComplete
{
    public class GetSalesSegmentAutoCompleteQueryHandler : IRequestHandler<GetSalesSegmentAutoCompleteQuery, IReadOnlyList<SalesSegmentLookupDto>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;

        public GetSalesSegmentAutoCompleteQueryHandler(ISalesSegmentQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<SalesSegmentLookupDto>> Handle(GetSalesSegmentAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
        }
    }
}
