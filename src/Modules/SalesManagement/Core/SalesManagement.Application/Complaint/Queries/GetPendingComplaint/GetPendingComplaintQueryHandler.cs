using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetPendingComplaint
{
    public class GetPendingComplaintQueryHandler : IRequestHandler<GetPendingComplaintQuery, ApiResponseDTO<List<ComplaintHeaderDto>>>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPendingComplaintQueryHandler(IComplaintQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ComplaintHeaderDto>>> Handle(GetPendingComplaintQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingComplaint",
                actionCode: "COMPLAINT_PENDING",
                actionName: data.Count.ToString(),
                details: "Pending Complaints were fetched.",
                module: "Complaint");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ComplaintHeaderDto>>
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
