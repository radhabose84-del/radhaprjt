using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Arrival.Common;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Arrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Commands.CreateArrival
{
    public class CreateArrivalCommandHandler : IRequestHandler<CreateArrivalCommand, ApiResponseDTO<int>>
    {
        private readonly IArrivalCommandRepository _commandRepository;
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;

        public CreateArrivalCommandHandler(
            IArrivalCommandRepository commandRepository,
            IArrivalQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateArrivalCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ArrivalHeader>(request);

            var unitId = _ipAddressService.GetUnitId()
                ?? throw new ExceptionRules("UnitId is not available for the current user.");
            entity.UnitId = unitId;
            entity.ArrivalDate = request.ArrivalDate == default ? DateTimeOffset.UtcNow : request.ArrivalDate;

            // Generate ArrivalNumber from DocumentSequence (TransactionType master)
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeArrival, MiscEnumEntity.ModulePurchase, unitId)
                ?? throw new ExceptionRules("No transaction type configured for Arrival.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
            entity.ArrivalNumber = sequences.Count > 0
                ? sequences[^1]
                : throw new ExceptionRules("No document sequence configured for Arrival.");

            entity.ArrivalDetails = request.Details.Select(MapDetail).ToList();

            // Build StockLedgerRaw rows (individual bales from payload, or expanded consolidated range).
            entity.StockRows = ArrivalStockLedgerFactory.Build(
                entity.ArrivalDate,
                entity.NetWeight,
                request.Details.Select(d => new ArrivalStockLedgerFactory.LineInput(
                    d.ItemId, d.UomId, d.BaleNumberFrom, d.BaleNumberTo,
                    d.BaleDetails?.Select(b => new ArrivalStockLedgerFactory.BaleEntry(
                        b.BaleNumber, b.BaleWeight, b.BaleCaptureMethodId, b.BarcodeNumber)).ToList())).ToList());

            var newId = await _commandRepository.CreateAsync(entity, transactionTypeId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ARRIVAL_CREATE",
                actionName: entity.ArrivalNumber,
                details: $"Arrival '{entity.ArrivalNumber}' created successfully with Id {newId}.",
                module: "Arrival");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Arrival created successfully.",
                Data = newId
            };
        }

        private static ArrivalDetail MapDetail(CreateArrivalDetailDto d) => new()
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
