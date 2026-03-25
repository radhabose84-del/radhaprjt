using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetAllVehicleMovementRecord
{
    public class GetAllVehicleMovementRecordQueryHandler : IRequestHandler<GetAllVehicleMovementRecordQuery, ApiResponseDTO<List<VehicleMovementRecordDto>>>
    {
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVehicleMovementRecordQueryHandler(IVehicleMovementRecordQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VehicleMovementRecordDto>>> Handle(GetAllVehicleMovementRecordQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVehicleMovementRecordQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "VehicleMovementRecord details were fetched.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VehicleMovementRecordDto>>
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
