using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProductionPack.Commands.UpdateProduction
{
    public class UpdateProductionCommandHandler
        : IRequestHandler<UpdateProductionCommand, ApiResponseDTO<int>>
    {
        private readonly IProductionCommandRepository _commandRepository;
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public UpdateProductionCommandHandler(
            IProductionCommandRepository commandRepository,
            IProductionQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateProductionCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ProductionPackHeader>(request.ProductionPackDetails!);
            entity.UnitId = _ipAddressService.GetUnitId();

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PACK_ALLOCATION_UPDATE",
                actionName: request.ProductionPackDetails!.Id.ToString(),
                details: $"Pack Allocation with Id {request.ProductionPackDetails.Id} updated successfully.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Pack Allocation updated successfully.",
                Data = result
            };
        }
    }
}
