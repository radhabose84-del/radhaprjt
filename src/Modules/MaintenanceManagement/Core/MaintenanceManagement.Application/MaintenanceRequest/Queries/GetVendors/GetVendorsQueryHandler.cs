using Contracts.Common;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetVendors
{
    public class GetVendorsQueryHandler
        : IRequestHandler<GetVendorsQuery, ApiResponseDTO<List<SupplierLookupDto>>>
    {
        private readonly ISupplierLookup _supplierLookup;
        private readonly IMediator _mediator;

        public GetVendorsQueryHandler(ISupplierLookup supplierLookup, IMediator mediator)
        {
            _supplierLookup = supplierLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SupplierLookupDto>>> Handle(
            GetVendorsQuery request, CancellationToken cancellationToken)
        {
            var suppliers = await _supplierLookup.SearchSuppliersAsync(request.Term, cancellationToken);
            var data = suppliers.ToList();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "VENDOR_SEARCH",
                actionName: data.Count.ToString(),
                details: $"Vendor (supplier) list fetched from Party Master. Term: {request.Term}",
                module: "MaintenanceRequest"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SupplierLookupDto>>
            {
                IsSuccess = true,
                Message = data.Count > 0 ? "Success" : "No vendors found.",
                Data = data
            };
        }
    }
}
