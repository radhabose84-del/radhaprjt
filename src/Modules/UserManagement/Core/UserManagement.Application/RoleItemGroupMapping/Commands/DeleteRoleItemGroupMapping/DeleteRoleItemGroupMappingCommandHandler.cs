using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping
{
    public class DeleteRoleItemGroupMappingCommandHandler
        : IRequestHandler<DeleteRoleItemGroupMappingCommand, bool>
    {
        private readonly IRoleItemGroupMappingCommandRepository _commandRepository;
        private readonly IRoleItemGroupMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteRoleItemGroupMappingCommandHandler(
            IRoleItemGroupMappingCommandRepository commandRepository,
            IRoleItemGroupMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteRoleItemGroupMappingCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing is null || existing.IsDeleted is Enums.IsDelete.Deleted)
            {
                throw new ValidationException(
                    "Invalid Id. The specified RoleItemGroupMapping does not exist or is deleted.");
            }

            var entityToDelete = _mapper.Map<Domain.Entities.RoleItemGroupMapping>(request);
            var deleteResult = await _commandRepository.DeleteAsync(request.Id, entityToDelete);

            if (deleteResult > 0)
            {
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: $"RoleId:{existing.RoleId}_ItemGroupId:{existing.ItemGroupId}",
                    actionName: "RoleItemGroupMapping",
                    details: $"RoleItemGroupMapping Id {request.Id} deleted. RoleId={existing.RoleId}, ItemGroupId={existing.ItemGroupId}.",
                    module: "RoleItemGroupMapping"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return true;
            }

            throw new Exception("RoleItemGroupMapping deletion failed.");
        }
    }
}
