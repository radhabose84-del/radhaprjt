using AutoMapper;
using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetAllFreightMaster
{
    public class GetAllFreightMasterQueryHandler : IRequestHandler<GetAllFreightMasterQuery, ApiResponseDTO<List<FreightMasterDto>>>
    {
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllFreightMasterQueryHandler(IFreightMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FreightMasterDto>>> Handle(GetAllFreightMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<FreightMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllFreightMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "FreightMaster details were fetched.",
                module: "FreightMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<FreightMasterDto>>
            {
                IsSuccess = true,
                Message = "FreightMaster list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
