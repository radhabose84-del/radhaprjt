using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
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
        private readonly IDocumentSequenceQueryRepository _documentSequenceQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceQueryRepository documentSequenceQueryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceQueryRepository = documentSequenceQueryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
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

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate PriceCode from DocumentSequence
            var typeId = await _documentSequenceQueryRepository.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypePriceMaster, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'PriceMaster' not found for Sales module.");

            var sequences = await _documentSequenceQueryRepository.GenerateDocumentNumber(typeId.Value);
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
    }
}
