using MediatR;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete
{
    public class GetRfqAutoCompleteQuotationHandler
        : IRequestHandler<GetRfqAutoCompleteQuotationQuery, List<RfqAutoCompleteDto>>
    {
        private readonly IRfqQueryRepository _repo;
        public GetRfqAutoCompleteQuotationHandler(IRfqQueryRepository repo) => _repo = repo;

        public Task<List<RfqAutoCompleteDto>> Handle(
            GetRfqAutoCompleteQuotationQuery request, CancellationToken ct)
            => _repo.GetRfqAutoCompleteQuotationAsync(request.SearchPattern, request.LastSubmitDate, ct);
    }
}
