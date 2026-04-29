using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster
{
    public class UpdateSalesOrderTypeMasterCommandHandler
        : IRequestHandler<UpdateSalesOrderTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesOrderTypeMasterCommandHandler(
            ISalesOrderTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateSalesOrderTypeMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesOrderTypeMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALESORDERTYPEMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Order Type with Id {request.Id} updated successfully.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order Type updated successfully.",
                Data = updatedId
            };
        }
    }
}
