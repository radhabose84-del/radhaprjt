using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedback
{
    public class GetAllComplaintDepartmentFeedbackQueryHandler : IRequestHandler<GetAllComplaintDepartmentFeedbackQuery, ApiResponseDTO<List<FeedbackListDto>>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAllComplaintDepartmentFeedbackQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FeedbackListDto>>> Handle(GetAllComplaintDepartmentFeedbackQuery request, CancellationToken cancellationToken)
        {
            int? responsiblePersonId = request.MyPendingOnly ? _ipAddressService.GetUserId() : null;
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter, responsiblePersonId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAllComplaintDepartmentFeedbackQuery",
                actionName: data.Count.ToString(),
                details: "Department feedback list was fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FeedbackListDto>>
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
