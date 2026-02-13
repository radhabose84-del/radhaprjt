using MediatR;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoCompleteComparison
{
    public class GetRfqAutoCompleteComparisonHandler
        : IRequestHandler<GetRfqAutoCompleteComparisonQuery, List<RfqAutoCompleteDto>>
    {
        private readonly IRfqQueryRepository _repo;
        public GetRfqAutoCompleteComparisonHandler(IRfqQueryRepository repo) => _repo = repo;

        public Task<List<RfqAutoCompleteDto>> Handle(
            GetRfqAutoCompleteComparisonQuery request, CancellationToken ct)
            => _repo.GetRfqAutoCompleteComparisonAsync(request.SearchPattern, request.LastSubmitDate,request.StatusId, ct);
    }
}
