using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualitySpecification.Queries.GetAllQualitySpecification
{
    public class GetAllQualitySpecificationQueryHandler : IRequestHandler<GetAllQualitySpecificationQuery, ApiResponseDTO<List<QualitySpecificationListDto>>>
    {
        private readonly IQualitySpecificationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllQualitySpecificationQueryHandler(
            IQualitySpecificationQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QualitySpecificationListDto>>> Handle(GetAllQualitySpecificationQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                request.QualityTemplateId, request.ApplicableLevelId,
                request.ItemCategoryId, request.ItemId, request.IsActive);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQualitySpecificationQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "QualitySpecification details were fetched.",
                module: "QualitySpecification"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QualitySpecificationListDto>>
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
