#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup
{
    public class CreateSalesGroupCommandHandler : IRequestHandler<CreateSalesGroupCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesGroupCommandHandler(
            ISalesGroupCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesGroup>(request);
            entity.IsActive = Domain.Common.BaseEntity.Status.Active;
            entity.IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_GROUP_CREATE",
                actionName: request.SalesGroupName,
                details: $"Sales Group '{request.SalesGroupName}' created successfully with Id {newId}.",
                module: "SalesGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Group created successfully.",
                Data = newId
            };
        }
    }
}
