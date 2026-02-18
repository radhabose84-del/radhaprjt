using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using FluentValidation;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IModule;
using Contracts.Common;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Modules.Commands.DeleteModule
{
    public class DeleteModuleCommandHandler  : IRequestHandler<DeleteModuleCommand,bool>
    {
        private readonly IModuleCommandRepository _moduleRepository;
        private readonly IModuleQueryRepository _moduleQueryRepository;

        private readonly ILogger<DeleteModuleCommandHandler> _logger;


    public DeleteModuleCommandHandler(IModuleCommandRepository moduleRepository,IModuleQueryRepository moduleQueryRepository,ILogger<DeleteModuleCommandHandler> logger)
    {
        _moduleRepository = moduleRepository;
        _moduleQueryRepository = moduleQueryRepository;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    public async Task<bool> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogError("DeleteModuleCommand request is null.");
            throw new ArgumentNullException(nameof(request));
        }
             _logger.LogInformation("Starting module deletion process for ModuleId: {ModuleId}", request.ModuleId);
        // Check if the module exists
            var module = await _moduleQueryRepository.GetModuleByIdAsync(request.ModuleId);
            if (module == null || module.IsDeleted)
            {
                _logger.LogWarning("Module with ID {ModuleId} not found or already deleted.", request.ModuleId);
                throw new ValidationException("Module not found or already deleted.");
               
            }

        await _moduleRepository.DeleteModuleAsync(request.ModuleId);
        await _moduleRepository.SaveChangesAsync();
            _logger.LogInformation("Module with ID {ModuleId} successfully marked as deleted.", request.ModuleId);

            return true;
    }
    }
}