using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetAllOCREntry
{
    public class GetAllOCREntryQueryHandler : IRequestHandler<GetAllOCREntryQuery, ApiResponseDTO<List<OCREntryDto>>>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllOCREntryQueryHandler(IOCREntryQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<OCREntryDto>>> Handle(GetAllOCREntryQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllOCREntryQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "OCR details were fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<OCREntryDto>>
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
