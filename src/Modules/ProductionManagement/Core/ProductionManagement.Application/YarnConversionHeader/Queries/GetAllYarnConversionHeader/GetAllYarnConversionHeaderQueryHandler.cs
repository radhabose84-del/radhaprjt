using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Queries.GetAllYarnConversionHeader
{
    public class GetAllYarnConversionHeaderQueryHandler
        : IRequestHandler<GetAllYarnConversionHeaderQuery, ApiResponseDTO<List<YarnConversionHeaderDto>>>
    {
        private readonly IYarnConversionHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllYarnConversionHeaderQueryHandler(
            IYarnConversionHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<YarnConversionHeaderDto>>> Handle(
            GetAllYarnConversionHeaderQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllYarnConversionHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "YarnConversionHeader details were fetched.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<YarnConversionHeaderDto>>
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
