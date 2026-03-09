using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
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
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IItemLookup _itemLookup;

        public CreateItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IItemPriceMasterQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator,
            IMapper mapper,
            IItemLookup itemLookup)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _itemLookup = itemLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);

            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            entity.StatusId = pendingStatus?.Id;

            // Auto-generate PriceCode: first 3 chars of ItemName (uppercase) + "-" + 3-digit serial
            var items = await _itemLookup.GetByIdsAsync(new[] { request.ItemId }, cancellationToken);
            var itemData = items.FirstOrDefault();
            var itemName = itemData?.ItemName ?? "UNK";
            var prefix = itemName.Length >= 3
                ? itemName[..3].ToUpperInvariant()
                : itemName.ToUpperInvariant().PadRight(3, 'X');

            var nextSerial = await _queryRepository.GetNextPriceCodeSerialAsync(prefix);
            entity.PriceCode = $"{prefix}-{nextSerial:D3}";

            var newId = await _commandRepository.CreateAsync(entity);

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
    }
}
