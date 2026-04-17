using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetAllComplaint
{
    public class GetAllComplaintQueryHandler : IRequestHandler<GetAllComplaintQuery, ApiResponseDTO<List<ComplaintHeaderDto>>>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllComplaintQueryHandler(IComplaintQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ComplaintHeaderDto>>> Handle(GetAllComplaintQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllComplaintQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Complaint details were fetched.",
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
