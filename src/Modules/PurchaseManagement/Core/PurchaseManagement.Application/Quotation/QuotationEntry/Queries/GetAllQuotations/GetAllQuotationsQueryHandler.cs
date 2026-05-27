using AutoMapper;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetAllQuotations
{
    public class GetAllQuotationsQueryHandler
        : IRequestHandler<GetAllQuotationsQuery, (IReadOnlyList<QuotationListItemDto> Items, int Total)>
    {
        private readonly IQuotationQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ip;

        public GetAllQuotationsQueryHandler(
            IQuotationQueryRepository repo,
            IMapper mapper,
            IMediator mediator,
            IIPAddressService ip)
        {
            _repo     = repo;
            _mapper   = mapper;
            _mediator = mediator;
            _ip       = ip;
        }

        public async Task<(IReadOnlyList<QuotationListItemDto> Items, int Total)> Handle(
            GetAllQuotationsQuery request,
            CancellationToken ct)
        {
            // Supplier-scoped view when the JWT carries a PartyId (supplier portal login).
            // Buyer / internal users have no PartyId → fall back to the full list.
            var partyId = _ip.GetPartyId();

            // 🔹 IMPORTANT: do NOT pass SearchTerm to repo if you want SupplierName search to work
            var (items, total) = (partyId.HasValue && partyId.Value > 0)
                ? await _repo.GetMyQuotationsAsync(
                    supplierPartyId: partyId.Value,
                    PageNumber: request.PageNumber,
                    PageSize:   request.PageSize,
                    SearchTerm: null)
                : await _repo.GetAllAsync(
                    PageNumber: request.PageNumber,
                    PageSize:   request.PageSize,
                    SearchTerm: null);

            // repo already returns QuotationListItemDto, mapping is optional
            var result = _mapper.Map<List<QuotationListItemDto>>(items);

            // 🔍 In-memory filter on QuotationNumber + SupplierName
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLowerInvariant();

                result = result
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.QuotationNumber) &&
                        x.QuotationNumber.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(x.SupplierName) &&
                        x.SupplierName.ToLower().Contains(term)) ||
                         (!string.IsNullOrEmpty(x.RfqNumber) &&
                        x.RfqNumber.ToLower().Contains(term))
                    )
                    .ToList();
            }

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode:   "GetAllQuotationsQuery",
                actionName:   total.ToString(), // keep logging total from DB
                details:      $"Quotation list fetched. Page={request.PageNumber}, Size={request.PageSize}, Total={total}",
                module:       "QuotationEntry"
            );

            await _mediator.Publish(domainEvent, ct);

            return (result, total); // Total = DB total (like your PriceMaster pattern)
        }

    }
}
