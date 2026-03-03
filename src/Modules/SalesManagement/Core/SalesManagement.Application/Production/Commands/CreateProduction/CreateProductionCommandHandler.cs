using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProduction;
using SalesManagement.Application.Production.Dto;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Production.Commands.CreateProduction
{
    public class CreateProductionCommandHandler
        : IRequestHandler<CreateProductionCommand, ApiResponseDTO<int>>
    {
        private readonly IProductionCommandRepository _commandRepository;
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateProductionCommandHandler(
            IProductionCommandRepository commandRepository,
            IProductionQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateProductionCommand request,
            CancellationToken cancellationToken)
        {
            var details = request.ProductionPackDetails!;

            var entity = _mapper.Map<ProductionPackHeader>(details);

            // Auto-generate PackNo
            var packAllocationNo = await _commandRepository.GenerateNextPackNoAsync(
                details.WarehouseId, cancellationToken);
            entity.PackNo = packAllocationNo;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PACK_ALLOCATION_CREATE",
                actionName: packAllocationNo,
                details: $"Pack Allocation '{packAllocationNo}' created successfully with Id {newId}.",
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
