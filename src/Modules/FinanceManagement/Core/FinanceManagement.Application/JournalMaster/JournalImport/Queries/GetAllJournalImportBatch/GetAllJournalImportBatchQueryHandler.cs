using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetAllJournalImportBatch
{
    public class GetAllJournalImportBatchQueryHandler : IRequestHandler<GetAllJournalImportBatchQuery, ApiResponseDTO<List<JournalImportBatchDto>>>
    {
        private readonly IJournalImportQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllJournalImportBatchQueryHandler(IJournalImportQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalImportBatchDto>>> Handle(GetAllJournalImportBatchQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllBatchesAsync(request.PageNumber, request.PageSize);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllJournalImportBatchQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal import batch details were fetched.",
                module: "JournalImport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalImportBatchDto>>
            {
                IsSuccess = true,
                Message = "Journal import batch list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
