using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetCustomerInvoices
{
    public class GetCustomerInvoicesQueryHandler : IRequestHandler<GetCustomerInvoicesQuery, List<CustomerInvoiceDto>>
    {
        private readonly IComplaintQueryRepository _queryRepository;

        public GetCustomerInvoicesQueryHandler(IComplaintQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<CustomerInvoiceDto>> Handle(GetCustomerInvoicesQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetCustomerInvoicesAsync(request.CustomerId);
        }
    }
}
