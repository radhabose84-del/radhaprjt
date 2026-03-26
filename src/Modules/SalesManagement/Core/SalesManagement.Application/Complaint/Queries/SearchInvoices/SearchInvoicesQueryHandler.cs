using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.SearchInvoices
{
    public class SearchInvoicesQueryHandler : IRequestHandler<SearchInvoicesQuery, ApiResponseDTO<List<InvoiceSearchDto>>>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public SearchInvoicesQueryHandler(IComplaintQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<InvoiceSearchDto>>> Handle(SearchInvoicesQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.SearchInvoicesAsync(
                request.PartyId, request.SearchTerm, request.LastOneYear, request.PageNumber, request.PageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "SearchInvoices",
                actionCode: "COMPLAINT_SEARCH_INVOICES",
                actionName: data.Count.ToString(),
                details: $"Invoice search for PartyId {request.PartyId} returned {data.Count} results.",
                module: "Complaint");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<InvoiceSearchDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
