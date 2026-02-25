using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete
{
    public class GetPaymentTermAutoCompleteQueryHandler : IRequestHandler<GetPaymentTermAutoCompleteQuery, List<AutoCompleteDto>>
    {
        private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;

        public GetPaymentTermAutoCompleteQueryHandler(IPaymentTermMasterQueryRepository paymentTermMasterQueryRepository)
        {
            _paymentTermMasterQueryRepository = paymentTermMasterQueryRepository;
        }
          
            public async Task<List<AutoCompleteDto>> Handle(GetPaymentTermAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var search = request.SearchPattern?.Trim();
            var exactCode = request.PaymentTermCode?.Trim();

            var items = await _paymentTermMasterQueryRepository.GetPaymentTermAutoComplete(search, exactCode);
            return items ?? new List<AutoCompleteDto>();
        }
    }
}