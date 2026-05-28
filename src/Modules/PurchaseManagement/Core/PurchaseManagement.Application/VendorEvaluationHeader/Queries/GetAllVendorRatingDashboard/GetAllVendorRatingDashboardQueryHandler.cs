using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorRatingDashboard
{
    public class GetAllVendorRatingDashboardQueryHandler : IRequestHandler<GetAllVendorRatingDashboardQuery, ApiResponseDTO<VendorRatingDashboardResponseDto>>
    {
        private readonly IVendorEvaluationDashboardQueryRepository _dashboardRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVendorRatingDashboardQueryHandler(
            IVendorEvaluationDashboardQueryRepository dashboardRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _dashboardRepository = dashboardRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<VendorRatingDashboardResponseDto>> Handle(
            GetAllVendorRatingDashboardQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount, summary) = await _dashboardRepository.GetAllDashboardAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.Grade);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVendorRatingDashboard",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Vendor Rating Dashboard summary list was fetched.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<VendorRatingDashboardResponseDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = new VendorRatingDashboardResponseDto
                {
                    Summary = summary,
                    Items = data
                },
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
