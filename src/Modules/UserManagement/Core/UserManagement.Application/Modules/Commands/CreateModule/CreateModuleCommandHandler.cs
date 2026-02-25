using FluentValidation;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Modules.Commands.CreateModule
{
    public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, int>
    {
        private readonly IModuleCommandRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateModuleCommandHandler> _logger;


    public CreateModuleCommandHandler(IModuleCommandRepository moduleRepository, IMapper mapper, IMediator mediator,ILogger<CreateModuleCommandHandler> logger)
    {
        _moduleRepository = moduleRepository;
        _mapper = mapper;
        _mediator = mediator;    
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }
    public async Task<int> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            {
                _logger.LogError("CreateModuleCommand request is null.");
                throw new ArgumentNullException(nameof(request));
            }

         _logger.LogInformation("Starting module creation process for ModuleName: {ModuleName}", request.ModuleName);
        // Validate inputs
        if (string.IsNullOrEmpty(request.ModuleName))
        {
            _logger.LogWarning("Validation failed: Module name is required.");
            throw new ValidationException("Module name is required.");
        }    
        // if (request.Menus == null || !request.Menus.Any())
        // {   
        //     _logger.LogWarning("Validation failed: At least one menu is required.");
        //     throw new ValidationException("At least one menu is required.");
        // }
        // // Map to domain model
        var module = new Domain.Entities.Modules
        {
            ModuleName = request.ModuleName,
            IsDeleted = false
        };
        await _moduleRepository.AddModuleAsync(module);
        await _moduleRepository.SaveChangesAsync();

        //Publish Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: request.ModuleName,
                actionName: request.ModuleName,
                details: $"Module '{request.ModuleName}' was created",
                module:"Modules"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation("Module successfully created with ID: {ModuleId}", module.Id);


            return module.Id;
    }
    }
}