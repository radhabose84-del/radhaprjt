using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice
{
    public class CreateDispatchAdviceCommandHandler : IRequestHandler<CreateDispatchAdviceCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAdviceCommandRepository _commandRepository;
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDispatchAdviceCommandHandler(
            IDispatchAdviceCommandRepository commandRepository,
            IDispatchAdviceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDispatchAdviceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DispatchAdviceHeader>(request);

            // Set Draft status
            var draftStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Pending);
            entity.StatusId = draftStatus?.Id ?? 0;

            // Generate auto-number (unit-based from SalesOrder)
            var unitId = await _queryRepository.GetSalesOrderUnitIdAsync(request.SalesOrderId);
            var dispatchNo = await _commandRepository.GenerateNextDispatchNoAsync(unitId, cancellationToken);
            entity.DispatchNo = dispatchNo;

            // Resolve Packed and Dispatched status IDs for StockLedger update
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Dispatched);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            // CreateAsync inserts header + details and updates StockLedger per PackNo
            var newId = await _commandRepository.CreateAsync(entity, unitId, packedStatusId, dispatchedStatusId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DISPATCHADVICE_CREATE",
                actionName: dispatchNo,
                details: $"Dispatch Advice '{dispatchNo}' created successfully with Id {newId}.",
                module: "DispatchAdvice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Advice created successfully.",
                Data = newId
            };
        }
    }
}
