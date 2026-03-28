using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetAllYarnTwistMaster
{
    public class GetAllYarnTwistMasterQueryHandler : IRequestHandler<GetAllYarnTwistMasterQuery, ApiResponseDTO<List<YarnTwistMasterDto>>>
    {
        private readonly IYarnTwistMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllYarnTwistMasterQueryHandler(
            IYarnTwistMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<YarnTwistMasterDto>>> Handle(GetAllYarnTwistMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<YarnTwistMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllYarnTwistMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Yarn Twist Master details were fetched.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<YarnTwistMasterDto>>
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
