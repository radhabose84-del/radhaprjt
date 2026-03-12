using AutoMapper;
using Contracts.Common;
using MediatR;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProductionPack.Commands.CreateProduction
{
    public class CreateProductionCommandHandler
        : IRequestHandler<CreateProductionCommand, ApiResponseDTO<int>>
    {
        private readonly IProductionCommandRepository _commandRepository;
        private readonly IDocumentSequenceQueryRepository _documentSequenceQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateProductionCommandHandler(
            IProductionCommandRepository commandRepository,
            IDocumentSequenceQueryRepository documentSequenceQueryRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _documentSequenceQueryRepository = documentSequenceQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateProductionCommand request,
            CancellationToken cancellationToken)
        {
            var details = request.ProductionPackDetails!;

            var entity = _mapper.Map<ProductionPackHeader>(details);
            entity.UnitId = _ipAddressService.GetUnitId() ?? 0;

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Generate PackNo from DocumentSequence
            var typeId = await _documentSequenceQueryRepository.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypePackMaster, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'PackMaster' not found for Sales module.");

            var sequences = await _documentSequenceQueryRepository.GenerateDocumentNumber(typeId.Value);
            var packNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.PackNo = packNo
                ?? throw new ExceptionRules("No document sequence configured for PackMaster.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PACK_ALLOCATION_CREATE",
                actionName: entity.PackNo,
                details: $"Pack Allocation '{entity.PackNo}' created successfully with Id {newId}.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Pack Allocation created successfully.",
                Data = newId
            };
        }
    }
}
