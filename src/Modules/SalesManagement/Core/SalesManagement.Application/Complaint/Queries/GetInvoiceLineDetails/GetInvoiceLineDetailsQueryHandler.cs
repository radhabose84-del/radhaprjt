using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetInvoiceLineDetails
{
    public class GetInvoiceLineDetailsQueryHandler : IRequestHandler<GetInvoiceLineDetailsQuery, List<InvoiceLineDetailDto>>
    {
        private readonly IComplaintQueryRepository _queryRepository;

        public GetInvoiceLineDetailsQueryHandler(IComplaintQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<InvoiceLineDetailDto>> Handle(GetInvoiceLineDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetInvoiceLineDetailsAsync(request.InvoiceHeaderId);
        }
    }
}
