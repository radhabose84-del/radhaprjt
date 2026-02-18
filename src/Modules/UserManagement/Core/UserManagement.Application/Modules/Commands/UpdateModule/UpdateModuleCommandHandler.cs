#nullable disable
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using FluentValidation;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Domain.Events;
using Contracts.Common;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Modules.Commands.UpdateModule
{
    public class UpdateModuleCommandHandler: IRequestHandler<UpdateModuleCommand,bool>
    {
    private readonly IModuleCommandRepository _moduleRepository;
    private readonly IModuleQueryRepository _moduleQueryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator; 
    private readonly ILogger<UpdateModuleCommandHandler> _logger;


    public UpdateModuleCommandHandler(IModuleCommandRepository moduleRepository, IMapper mapper, IModuleQueryRepository moduleQueryRepository, IMediator mediator,ILogger<UpdateModuleCommandHandler> logger)
    {
        _moduleRepository = moduleRepository;
        _mapper = mapper;
        _moduleQueryRepository = moduleQueryRepository;
        _mediator = mediator;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
           if (request == null)
    {
        _logger.LogError("UpdateModuleCommand request is null.");
        throw new ArgumentNullException(nameof(request));
    }

    _logger.LogInformation("Starting module update process for ModuleId: {ModuleId}", request.ModuleId);

   var module = await _moduleQueryRepository.GetModuleByIdAsync(request.ModuleId);
    if (module == null || module.IsDeleted)
    {
        _logger.LogWarning("Module with ID {ModuleId} not found or has been deleted.", request.ModuleId);
        throw new ValidationException("Module not found or has been deleted.");
       
    }

    // var oldModuleName = module.ModuleName; // Store the old name
    // module.ModuleName = request.ModuleName; // Update name

            // Update menus
            // var existingMenus = module.Menus.Select(m => m.MenuName).ToList();
            // var menusToAdd = request.Menus.Except(existingMenus).ToList();
            // var menusToRemove = existingMenus.Except(request.Menus).ToList();

            // module.Menus.ToList().RemoveAll(m => menusToRemove.Contains(m.MenuName)); // Remove unwanted menus
            // foreach (var menuName in menusToAdd)
            // {
            //     module.Menus.Add(new Domain.Entities.Menu { MenuName = menuName }); // Add new menus
            // }
    //   var moduledata = new Domain.Entities.Modules
    //     {
    //         Id = request.ModuleId,
    //         ModuleName = request.ModuleName
    //     };
        var moduledata  = _mapper.Map<Domain.Entities.Modules>(request);

    // Publish Domain Event
            var domainEvent = new AuditLogsDomainEvent(
        actionDetail: "Update",
        actionCode: module.ModuleName,
        actionName: module.ModuleName,
        details: $"Module '{module.ModuleName}' was updated to '{module.ModuleName}'.",
        module: "Module"
    );

    await _mediator.Publish(domainEvent, cancellationToken);

#pragma warning disable CS4014


#pragma warning restore CS4014
    #pragma warning disable CS4014
    _moduleRepository.UpdateModuleAsync(moduledata); // Ensure EF Core tracks changes
    #pragma warning restore CS4014
    // await _moduleRepository.SaveChangesAsync();

    _logger.LogInformation("Module with ID {ModuleId} successfully updated.", request.ModuleId);

    return true;
    }
}
}