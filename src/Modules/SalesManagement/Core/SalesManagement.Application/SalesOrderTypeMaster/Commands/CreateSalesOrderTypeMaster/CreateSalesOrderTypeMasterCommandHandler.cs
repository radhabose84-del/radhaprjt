using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster
{
    public class CreateSalesOrderTypeMasterCommandHandler
        : IRequestHandler<CreateSalesOrderTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesOrderTypeMasterCommandHandler(
            ISalesOrderTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateSalesOrderTypeMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesOrderTypeMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESORDERTYPEMASTER_CREATE",
                actionName: request.TypeName ?? string.Empty,
                details: $"Sales Order Type '{request.TypeName}' created successfully with Id {newId}.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order Type created successfully.",
                Data = newId
            };
        }
    }
}
