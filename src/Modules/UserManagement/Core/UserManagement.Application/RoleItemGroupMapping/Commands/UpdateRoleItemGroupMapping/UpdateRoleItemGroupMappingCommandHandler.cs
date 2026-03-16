using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping
{
    public class UpdateRoleItemGroupMappingCommandHandler
        : IRequestHandler<UpdateRoleItemGroupMappingCommand, RoleItemGroupMappingDto>
    {
        private readonly IRoleItemGroupMappingCommandRepository _commandRepository;
        private readonly IRoleItemGroupMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateRoleItemGroupMappingCommandHandler(
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

        public async Task<RoleItemGroupMappingDto> Handle(
            UpdateRoleItemGroupMappingCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing is null || existing.IsDeleted is Enums.IsDelete.Deleted)
            {
                throw new ValidationException(
                    "Invalid Id. The specified RoleItemGroupMapping does not exist or is deleted.");
            }

            // Check composite key uniqueness (exclude current record)
            var compositeExists = await _commandRepository.CompositeKeyExistsAsync(
                request.RoleId, request.ItemGroupId, request.Id);
            if (compositeExists)
            {
                throw new ValidationException(
                    "RoleItemGroupMapping with the same RoleId and ItemGroupId already exists.");
            }

            var updatedEntity = _mapper.Map<Domain.Entities.RoleItemGroupMapping>(request);
            var updateResult = await _commandRepository.UpdateAsync(request.Id, updatedEntity);

            var refreshed = await _queryRepository.GetByIdAsync(request.Id);
            if (refreshed != null)
            {
                var dto = _mapper.Map<RoleItemGroupMappingDto>(refreshed);

                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: $"RoleId:{request.RoleId}_ItemGroupId:{request.ItemGroupId}",
                    actionName: "RoleItemGroupMapping",
                    details: $"RoleItemGroupMapping Id {request.Id} updated. RoleId={request.RoleId}, ItemGroupId={request.ItemGroupId}.",
                    module: "RoleItemGroupMapping"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                if (updateResult > 0)
                {
                    return dto;
                }
                throw new Exception("RoleItemGroupMapping not updated.");
            }
            else
            {
                throw new ValidationException("RoleItemGroupMapping not found.");
            }
        }
    }
}
