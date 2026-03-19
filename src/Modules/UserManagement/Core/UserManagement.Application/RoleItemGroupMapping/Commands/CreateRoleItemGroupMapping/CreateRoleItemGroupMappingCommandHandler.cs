using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping
{
    public class CreateRoleItemGroupMappingCommandHandler
        : IRequestHandler<CreateRoleItemGroupMappingCommand, RoleItemGroupMappingDto>
    {
        private readonly IMapper _mapper;
        private readonly IRoleItemGroupMappingCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public CreateRoleItemGroupMappingCommandHandler(
            IMapper mapper,
            IRoleItemGroupMappingCommandRepository commandRepository,
            IMediator mediator)
        {
            _mapper = mapper;
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<RoleItemGroupMappingDto> Handle(
            CreateRoleItemGroupMappingCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _commandRepository.CompositeKeyExistsAsync(request.RoleId, request.ItemGroupId);
            if (exists)
            {
                throw new ValidationException(
                    "RoleItemGroupMapping with the same RoleId and ItemGroupId already exists.");
            }

            var entity = _mapper.Map<Domain.Entities.RoleItemGroupMapping>(request);
            var result = await _commandRepository.CreateAsync(entity);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: $"RoleId:{result.RoleId}_ItemGroupId:{result.ItemGroupId}",
                actionName: "RoleItemGroupMapping",
                details: $"RoleItemGroupMapping created: RoleId={result.RoleId}, ItemGroupId={result.ItemGroupId}, Id={result.Id}.",
                module: "RoleItemGroupMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            var dto = _mapper.Map<RoleItemGroupMappingDto>(result);
            return dto;
        }
    }
}
