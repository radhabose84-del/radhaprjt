using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetAllRepackingMaster
{
    public class GetAllRepackingMasterQueryHandler
        : IRequestHandler<GetAllRepackingMasterQuery, ApiResponseDTO<List<RepackingMasterDto>>>
    {
        private readonly IRepackingMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRepackingMasterQueryHandler(
            IRepackingMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RepackingMasterDto>>> Handle(
            GetAllRepackingMasterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRepackingMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "RepackingMaster details were fetched.",
                module: "RepackingMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RepackingMasterDto>>
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
