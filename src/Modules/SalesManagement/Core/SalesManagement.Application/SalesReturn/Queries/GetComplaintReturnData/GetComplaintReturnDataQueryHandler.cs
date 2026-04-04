using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesReturn.Queries.GetComplaintReturnData
{
    public class GetComplaintReturnDataQueryHandler : IRequestHandler<GetComplaintReturnDataQuery, ApiResponseDTO<ComplaintReturnDataDto>>
    {
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetComplaintReturnDataQueryHandler(ISalesReturnQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ComplaintReturnDataDto>> Handle(GetComplaintReturnDataQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetComplaintReturnDataAsync(request.ComplaintHeaderId);

            if (data == null)
                return new ApiResponseDTO<ComplaintReturnDataDto>
                {
                    IsSuccess = false,
                    Message = "Complaint not found or not eligible for return.",
                    Data = null
                };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetComplaintReturnData",
                actionCode: "GetComplaintReturnDataQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Complaint return data for {request.ComplaintHeaderId} was fetched.",
                module: "SalesReturn");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<ComplaintReturnDataDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
