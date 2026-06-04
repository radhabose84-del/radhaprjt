using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Station.Queries.GetAllStation
{
    public class GetAllStationQueryHandler : IRequestHandler<GetAllStationQuery, ApiResponseDTO<List<GetAllStationDto>>>
    {
        private readonly IStationQueryRepository _stationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllStationQueryHandler(IStationQueryRepository stationRepository, IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _stationRepository = stationRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetAllStationDto>>> Handle(GetAllStationQuery request, CancellationToken cancellationToken)
        {
            var (stations, totalCount) = await _stationRepository
                .GetAllStationAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (stations == null || !stations.Any())
            {
                return new ApiResponseDTO<List<GetAllStationDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found"
                };
            }

            var stationList = _mapper.Map<List<GetAllStationDto>>(stations);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Station details were fetched.",
                module: "Station");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetAllStationDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = stationList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
