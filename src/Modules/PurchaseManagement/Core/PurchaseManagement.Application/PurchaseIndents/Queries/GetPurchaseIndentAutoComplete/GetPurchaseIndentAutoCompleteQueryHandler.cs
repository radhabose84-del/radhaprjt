using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete
{
    public class GetPurchaseIndentAutoCompleteQueryHandler : IRequestHandler<GetPurchaseIndentAutoCompleteQuery, List<PurchaseIndentAutoCompleteQueryDto>>
    {
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IMapper _mapper;
        public GetPurchaseIndentAutoCompleteQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMapper mapper)
        {
            _purchaseIndentQuery = purchaseIndentQuery;
            _mapper = mapper;
        }
        public async Task<List<PurchaseIndentAutoCompleteQueryDto>> Handle(GetPurchaseIndentAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _purchaseIndentQuery.GetPurchaseIndentAutoCompleteAsync(request.Status ?? string.Empty, request.SearchTerm,request.AllIndents);

            var Indent = _mapper.Map<List<PurchaseIndentAutoCompleteQueryDto>>(result);
            
            return Indent;
        }
    }
}