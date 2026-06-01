using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QcInspection.Queries.GetAllQcInspection
{
    public class GetAllQcInspectionQueryHandler : IRequestHandler<GetAllQcInspectionQuery, ApiResponseDTO<List<QcInspectionListDto>>>
    {
        private readonly IQcInspectionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllQcInspectionQueryHandler(IQcInspectionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QcInspectionListDto>>> Handle(GetAllQcInspectionQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                request.QcStatusId, request.InspectionDateFrom, request.InspectionDateTo);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQcInspectionQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "QC Inspection details were fetched.",
                module: "QcInspection"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QcInspectionListDto>>
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
