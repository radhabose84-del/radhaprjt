using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Queries.GetAllSalesReturn
{
    public class GetAllSalesReturnQueryHandler : IRequestHandler<GetAllSalesReturnQuery, ApiResponseDTO<List<SalesReturnListDto>>>
    {
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSalesReturnQueryHandler(ISalesReturnQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesReturnListDto>>> Handle(GetAllSalesReturnQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.StatusFilter,
                request.FromDate,
                request.ToDate,
                request.CustomerId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAllSalesReturnQuery",
                actionName: data.Count.ToString(),
                details: "Sales Return list was fetched.",
                module: "SalesReturn");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesReturnListDto>>
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
