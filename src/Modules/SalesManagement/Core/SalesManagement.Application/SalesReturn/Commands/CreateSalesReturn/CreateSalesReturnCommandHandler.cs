using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn
{
    public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesReturnCommandRepository _commandRepository;
        private readonly ISalesReturnQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesReturnCommandHandler(
            ISalesReturnCommandRepository commandRepository,
            ISalesReturnQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesReturnCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesReturnHeader>(request);

            // Set Pending status
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ReturnStatus, MiscEnumEntity.ReturnStatusPending);
            entity.StatusId = pendingStatus?.Id ?? 0;

            // Map details
            if (request.Details != null && request.Details.Count > 0)
            {
                entity.SalesReturnDetails = new List<SalesReturnDetail>();
                foreach (var detail in request.Details)
                {
                    var detailEntity = _mapper.Map<SalesReturnDetail>(detail);
                    entity.SalesReturnDetails.Add(detailEntity);
                }
            }

            // Generate Return Number
            var unitId = _ipAddressService.GetUnitId();
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSalesReturn, MiscEnumEntity.ModuleSales, unitId ?? 0);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Sales Return' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var returnNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.ReturnNumber = returnNumber
                ?? throw new ExceptionRules("No document sequence configured for Sales Return.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            // Insert StockLedger entries (one per pack)
            if (request.Details != null && request.Details.Count > 0)
            {
                // Get StockEntryType Id for Sales Return
                var stockEntryType = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.StockEntryType, MiscEnumEntity.StockEntryTypeSalesReturn);
                var typeId2 = stockEntryType?.Id;

                var stockEntries = new List<Domain.Entities.StockLedger>();
                foreach (var detail in entity.SalesReturnDetails!)
                {
                    for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                    {
                        stockEntries.Add(new Domain.Entities.StockLedger
                        {
                            UnitId = unitId ?? 0,
                            DocType = "SR",
                            DocNo = newId,
                            DetailDocNo = detail.Id,
                            DocDate = request.ReturnDate,
                            ItemId = detail.ItemId,
                            LotId = detail.LotId ?? 0,
                            PackNo = packNo,
                            PackTypeId = detail.PackTypeId ?? 0,
                            WarehouseId = request.WarehouseId,
                            BinId = request.BinId,
                            TotalQty = 1,
                            TotalValue = 0,
                            StatusId = detail.BagStatusId,
                            TypeId = typeId2
                        });
                    }
                }

                if (stockEntries.Count > 0)
                    await _commandRepository.InsertStockLedgerEntriesAsync(stockEntries);
            }

            // Update ComplaintResolution return status
            var (totalDispatched, totalReturned) = await _queryRepository.GetReturnProgressAsync(request.ComplaintHeaderId);
            var returnStatusCode = totalReturned >= totalDispatched
                ? MiscEnumEntity.ReturnStatusFullyReturned
                : MiscEnumEntity.ReturnStatusPartiallyReturned;

            var returnStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ReturnStatus, returnStatusCode);
            if (returnStatus != null)
            {
                await _commandRepository.UpdateComplaintResolutionReturnStatusAsync(
                    request.ComplaintHeaderId, returnStatus.Id, totalReturned);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_RETURN_CREATE",
                actionName: returnNumber,
                details: $"Sales Return '{returnNumber}' created for Complaint {request.ComplaintHeaderId} with Id {newId}.",
                module: "SalesReturn");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Return created successfully.",
                Data = newId
            };
        }
    }
}
