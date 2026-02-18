using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Modules.Queries.GetModuleById
{
    public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery,ModuleByIdDto>
    {
        private readonly IModuleQueryRepository _moduleQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
    
    public GetModuleByIdQueryHandler(IModuleQueryRepository moduleQueryRepository, IMapper mapper, IMediator mediator)
    {
        _moduleQueryRepository = moduleQueryRepository;
        _mapper = mapper;
        _mediator = mediator;
        
    }

        public async Task<ModuleByIdDto> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _moduleQueryRepository.GetModuleByIdAsync(request.Id);
             if (result == null)
            {
                throw new ValidationException("Module not found");
                
            }
            var modules = _mapper.Map<ModuleByIdDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "MOD_FETCH",        
                    actionName: "Fetch Module",
                    details: $"Module details for ModuleName {modules.ModuleName} were fetched.",
                    module:"Modules"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return modules;
        }
    }
}