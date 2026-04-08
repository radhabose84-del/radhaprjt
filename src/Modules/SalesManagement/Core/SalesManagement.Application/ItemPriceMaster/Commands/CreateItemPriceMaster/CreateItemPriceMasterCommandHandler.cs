using AutoMapper;
using Contracts.Common;
using MediatR;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster
{
    public class CreateItemPriceMasterCommandHandler
        : IRequestHandler<CreateItemPriceMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IItemPriceMasterCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IItemLookup itemLookup,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _itemLookup = itemLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            var statusId = pendingStatus?.Id;

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Get TransactionTypeId for PriceCode generation
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypePriceMaster, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'PriceMaster' not found for Sales module.");

            if (request.VariantId.HasValue)
            {
                // Single variant — create one record
                var entity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);
                entity.StatusId = statusId;

                var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
                var priceCode = sequences.Count > 0 ? sequences[^1] : null;
                entity.PriceCode = priceCode
                    ?? throw new ExceptionRules("No document sequence configured for PriceMaster.");

                var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: "ITEM_PRICE_CREATE",
                    actionName: entity.PriceCode,
                    details: $"Item Price Master '{entity.PriceCode}' created successfully with Id {newId}.",
                    module: "ItemPriceMaster"
                );
                await _mediator.Publish(auditEvent, cancellationToken);

                return new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Item Price Master created successfully.",
                    Data = newId
                };
            }
            else
            {
                // No VariantId — check for active variants under this parent item
                var variants = await _itemLookup.GetVariantsByParentIdAsync(request.ItemId, cancellationToken);

                if (variants.Count == 0)
                {
                    // No variants exist — create single record with VariantId = null
                    var entity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);
                    entity.StatusId = statusId;
                    entity.VariantId = null;

                    var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
                    var priceCode = sequences.Count > 0 ? sequences[^1] : null;
                    entity.PriceCode = priceCode
                        ?? throw new ExceptionRules("No document sequence configured for PriceMaster.");

                    var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

                    var auditEvent = new AuditLogsDomainEvent(
                        actionDetail: "Create",
                        actionCode: "ITEM_PRICE_CREATE",
                        actionName: entity.PriceCode,
                        details: $"Item Price Master '{entity.PriceCode}' created successfully with Id {newId}.",
                        module: "ItemPriceMaster"
                    );
                    await _mediator.Publish(auditEvent, cancellationToken);

                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = true,
                        Message = "Item Price Master created successfully.",
                        Data = newId
                    };
                }

                // Variants exist — create one record per variant
                var entities = new List<Domain.Entities.ItemPriceMaster>();
                foreach (var variant in variants)
                {
                    var variantEntity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);
                    variantEntity.StatusId = statusId;
                    variantEntity.VariantId = variant.Id;
                    entities.Add(variantEntity);
                }

                var newIds = await _commandRepository.CreateBulkAsync(entities, typeId.Value);

                var bulkAuditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: "ITEM_PRICE_BULK_CREATE",
                    actionName: $"{newIds.Count} records",
                    details: $"Item Price Master bulk created {newIds.Count} records for ItemId {request.ItemId} across all variants.",
                    module: "ItemPriceMaster"
                );
                await _mediator.Publish(bulkAuditEvent, cancellationToken);

                return new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = $"Item Price Master created successfully for {newIds.Count} variants.",
                    Data = newIds.FirstOrDefault()
                };
            }
        }
    }
}
