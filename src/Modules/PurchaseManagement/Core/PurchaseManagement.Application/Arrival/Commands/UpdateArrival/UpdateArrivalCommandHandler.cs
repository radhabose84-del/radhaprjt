using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Arrival.Common;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Commands.UpdateArrival
{
    public class UpdateArrivalCommandHandler : IRequestHandler<UpdateArrivalCommand, ApiResponseDTO<int>>
    {
        private readonly IArrivalCommandRepository _commandRepository;
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateArrivalCommandHandler(
            IArrivalCommandRepository commandRepository,
            IArrivalQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateArrivalCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ArrivalHeader>(request);

            // NetWeight / WeightDifference are taken from the payload (mapped via AutoMapper), not computed.

            entity.ArrivalDetails = request.Details.Select(MapDetail).ToList();

            // Rebuild StockLedgerRaw rows (individual bales from payload, or expanded consolidated range).
            entity.StockRows = ArrivalStockLedgerFactory.Build(
                entity.ArrivalDate,
                entity.NetWeight,
                request.Details.Select(d => new ArrivalStockLedgerFactory.LineInput(
                    d.ItemId, d.UomId, d.BaleNumberFrom, d.BaleNumberTo,
                    d.BaleDetails?.Select(b => new ArrivalStockLedgerFactory.BaleEntry(
                        b.BaleNumber, b.BaleWeight, b.BaleCaptureMethodId, b.BarcodeNumber)).ToList())).ToList());

            var result = await _commandRepository.UpdateAsync(entity, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ARRIVAL_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Arrival with Id {request.Id} updated successfully.",
                module: "Arrival");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Arrival updated successfully.",
                Data = result
            };
        }

        private static ArrivalDetail MapDetail(UpdateArrivalDetailDto d) => new()
        {
            ItemId = d.ItemId,
            HsnId = d.HsnId,
            PackTypeId = d.PackTypeId,
            MixCodeId = d.MixCodeId,
            UomId = d.UomId,
            Rate = d.Rate,
            OrderedQty = d.OrderedQty,
            ArrivedQty = d.ArrivedQty,
            CancelledQty = d.CancelledQty,
            BalanceQty = d.OrderedQty - d.ArrivedQty - d.CancelledQty,
            BatchNumber = d.BatchNumber,
            BaleNumberFrom = d.BaleNumberFrom,
            BaleNumberTo = d.BaleNumberTo,
            TotalBaleCount = d.TotalBaleCount
        };
    }
}
