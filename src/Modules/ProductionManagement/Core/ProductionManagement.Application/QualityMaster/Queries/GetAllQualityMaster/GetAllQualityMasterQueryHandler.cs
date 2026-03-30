using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Queries.GetAllQualityMaster
{
    public class GetAllQualityMasterQueryHandler : IRequestHandler<GetAllQualityMasterQuery, ApiResponseDTO<List<QualityMasterDto>>>
    {
        private readonly IQualityMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllQualityMasterQueryHandler(
            IQualityMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QualityMasterDto>>> Handle(GetAllQualityMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<QualityMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQualityMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Quality Master details were fetched.",
                module: "QualityMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QualityMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
