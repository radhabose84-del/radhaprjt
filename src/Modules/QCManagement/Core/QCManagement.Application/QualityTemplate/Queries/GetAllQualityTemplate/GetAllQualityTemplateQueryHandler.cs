using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityTemplate.Queries.GetAllQualityTemplate
{
    public class GetAllQualityTemplateQueryHandler : IRequestHandler<GetAllQualityTemplateQuery, ApiResponseDTO<List<QualityTemplateListDto>>>
    {
        private readonly IQualityTemplateQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllQualityTemplateQueryHandler(
            IQualityTemplateQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QualityTemplateListDto>>> Handle(GetAllQualityTemplateQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.IsActive);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQualityTemplateQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "QualityTemplate details were fetched.",
                module: "QualityTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QualityTemplateListDto>>
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
