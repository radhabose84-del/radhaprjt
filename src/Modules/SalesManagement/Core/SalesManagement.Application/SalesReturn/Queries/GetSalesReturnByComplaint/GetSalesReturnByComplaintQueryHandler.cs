using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint
{
    public class GetSalesReturnByComplaintQueryHandler : IRequestHandler<GetSalesReturnByComplaintQuery, ApiResponseDTO<SalesReturnHeaderDto>>
    {
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesReturnByComplaintQueryHandler(ISalesReturnQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesReturnHeaderDto>> Handle(GetSalesReturnByComplaintQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByComplaintIdAsync(request.ComplaintHeaderId);

            if (data == null)
                return new ApiResponseDTO<SalesReturnHeaderDto>
                {
                    IsSuccess = false,
                    Message = "No Sales Return found for this complaint.",
                    Data = null
                };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByComplaint",
                actionCode: "GetSalesReturnByComplaintQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Sales Return for complaint {request.ComplaintHeaderId} was fetched.",
                module: "SalesReturn");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesReturnHeaderDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
