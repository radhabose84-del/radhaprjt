using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup
{
    public class UpdateSalesGroupCommandHandler : IRequestHandler<UpdateSalesGroupCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesGroupCommandHandler(
            ISalesGroupCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesGroup>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_GROUP_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Group with Id {request.Id} updated successfully.",
                module: "SalesGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Group updated successfully.",
                Data = updatedId
            };
        }
    }
}
