using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetPostableJournals
{
    public class GetPostableJournalsQueryHandler : IRequestHandler<GetPostableJournalsQuery, ApiResponseDTO<List<JournalListItemDto>>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPostableJournalsQueryHandler(IJournalQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalListItemDto>>> Handle(GetPostableJournalsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetPostableAsync(request.PageNumber, request.PageSize, companyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPostableJournalsQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Postable journal vouchers were fetched.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalListItemDto>>
            {
                IsSuccess = true,
                Message = "Postable journal vouchers retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
