using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetAllProcessMaster
{
    public class GetAllProcessMasterQueryHandler : IRequestHandler<GetAllProcessMasterQuery, ApiResponseDTO<List<ProcessMasterDto>>>
    {
        private readonly IProcessMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllProcessMasterQueryHandler(
            IProcessMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProcessMasterDto>>> Handle(GetAllProcessMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<ProcessMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllProcessMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Process Master details were fetched.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProcessMasterDto>>
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
