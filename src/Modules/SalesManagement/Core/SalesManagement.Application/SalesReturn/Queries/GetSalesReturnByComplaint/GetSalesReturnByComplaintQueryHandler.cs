using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint
{
    public class GetSalesReturnByComplaintQueryHandler : IRequestHandler<GetSalesReturnByComplaintQuery, ApiResponseDTO<List<SalesReturnHeaderDto>>>
    {
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesReturnByComplaintQueryHandler(ISalesReturnQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesReturnHeaderDto>>> Handle(GetSalesReturnByComplaintQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetAllByComplaintIdAsync(request.ComplaintHeaderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByComplaint",
                actionCode: "GetSalesReturnByComplaintQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Sales Returns for complaint {request.ComplaintHeaderId} were fetched.",
                module: "SalesReturn");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesReturnHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
