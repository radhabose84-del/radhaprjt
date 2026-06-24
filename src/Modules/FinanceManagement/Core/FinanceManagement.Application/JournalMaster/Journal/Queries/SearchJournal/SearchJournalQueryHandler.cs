using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.SearchJournal
{
    public class SearchJournalQueryHandler : IRequestHandler<SearchJournalQuery, ApiResponseDTO<List<JournalListItemDto>>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public SearchJournalQueryHandler(IJournalQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalListItemDto>>> Handle(SearchJournalQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var filter = new JournalSearchFilter
            {
                VoucherNo = request.VoucherNo,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                AccountId = request.AccountId,
                CostCentreId = request.CostCentreId,
                AmountMin = request.AmountMin,
                AmountMax = request.AmountMax,
                VoucherTypeId = request.VoucherTypeId,
                StatusId = request.StatusId,
                CreatedBy = request.CreatedBy,
                ApprovedBy = request.ApprovedBy,
                SourceId = request.SourceId,
                Narration = request.Narration,
                Reference = request.Reference
            };

            var (data, totalCount) = await _queryRepository.SearchAsync(filter, request.PageNumber, request.PageSize, companyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "SearchJournalQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal voucher search was executed.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalListItemDto>>
            {
                IsSuccess = true,
                Message = "Journal voucher search results retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
