using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetResolutionFormData
{
    public class GetResolutionFormDataQueryHandler : IRequestHandler<GetResolutionFormDataQuery, ApiResponseDTO<ComplaintResolutionFormDataDto>>
    {
        private readonly IComplaintResolutionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetResolutionFormDataQueryHandler(IComplaintResolutionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ComplaintResolutionFormDataDto>> Handle(GetResolutionFormDataQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetFormDataByComplaintIdAsync(request.ComplaintHeaderId);

            if (data == null)
                return new ApiResponseDTO<ComplaintResolutionFormDataDto>
                {
                    IsSuccess = false,
                    Message = "Complaint not found or not eligible for resolution.",
                    Data = null
                };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFormData",
                actionCode: "GetResolutionFormDataQuery",
                actionName: request.ComplaintHeaderId.ToString(),
                details: $"Resolution form data for complaint {request.ComplaintHeaderId} was fetched.",
                module: "ComplaintResolution");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<ComplaintResolutionFormDataDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
