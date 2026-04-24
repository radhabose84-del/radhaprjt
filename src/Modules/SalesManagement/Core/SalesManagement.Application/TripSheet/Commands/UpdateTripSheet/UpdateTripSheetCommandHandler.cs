using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Commands.UpdateTripSheet
{
    public class UpdateTripSheetCommandHandler : IRequestHandler<UpdateTripSheetCommand, ApiResponseDTO<int>>
    {
        private readonly ITripSheetCommandRepository _commandRepository;
        private readonly ITripSheetQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateTripSheetCommandHandler(
            ITripSheetCommandRepository commandRepository,
            ITripSheetQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateTripSheetCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TripSheetHeader>(request);

            // Set UnitId from JWT token (not from payload)
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            entity.UnitId = unitId;

            var details = request.Details?.Select(d => new TripSheetDetail
            {
                DispatchAdviceHeaderId = d.DispatchAdviceHeaderId,
                SequenceNo = d.SequenceNo
            }).ToList() ?? new List<TripSheetDetail>();

            var result = await _commandRepository.UpdateAsync(entity, details);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "TRIPSHEET_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Trip Sheet with Id {request.Id} updated successfully.",
                module: "TripSheet"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Trip Sheet updated successfully.",
                Data = result
            };
        }
    }
}
