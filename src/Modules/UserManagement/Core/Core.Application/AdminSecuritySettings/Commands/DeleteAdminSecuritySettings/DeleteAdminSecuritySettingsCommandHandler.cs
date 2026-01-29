using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using Core.Domain.Events;
using Core.Application.Common;
using AutoMapper;
using MediatR;
using Core.Application.Common.Interfaces.IAdminSecuritySettings;
using System.Threading;
using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;
using FluentValidation;


namespace Core.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings
{
    public class DeleteAdminSecuritySettingsCommandHandler  : IRequestHandler< DeleteAdminSecuritySettingsCommand  ,int>
    {
          private readonly IAdminSecuritySettingsCommandRepository _IadminSecuritySettingsCommandRepository;  
       private readonly IMapper _Imapper;          
        private readonly IAdminSecuritySettingsQueryRepository  _IadminSecuritySettingsQueryRepository;
   
        private readonly IMediator _mediator; 
           private readonly ILogger<DeleteAdminSecuritySettingsCommandHandler> _logger;
       

        public DeleteAdminSecuritySettingsCommandHandler (IAdminSecuritySettingsCommandRepository adminSecuritySettingsCommandRepository ,IAdminSecuritySettingsQueryRepository adminSecuritySettingsQueryRepository ,IMediator mediator, IMapper mapper,ILogger<DeleteAdminSecuritySettingsCommandHandler> logger)
      {
         _IadminSecuritySettingsCommandRepository = adminSecuritySettingsCommandRepository;
         _Imapper = mapper;                       
          _IadminSecuritySettingsQueryRepository = adminSecuritySettingsQueryRepository;
          _mediator = mediator;
          _logger = logger;

      }

     public async Task<int> Handle(DeleteAdminSecuritySettingsCommand deleteAdminSecuritySettingsCommand, CancellationToken cancellationToken)
      {       

          _logger.LogInformation($"DeleteAdmin SecuritySettingsCommandHandler started for Admin Security Settings ID: {deleteAdminSecuritySettingsCommand.Id}");

            // Check if adminSecuritySettings exists
            var adminSecuritySettings = await _IadminSecuritySettingsQueryRepository.GetAdminSecuritySettingsByIdAsync(deleteAdminSecuritySettingsCommand.Id);
            if (adminSecuritySettings is null)
            {
                _logger.LogWarning($"Admin Security Settings with ID { deleteAdminSecuritySettingsCommand.Id} not found.");
                throw new ValidationException("Admin Security Settings not found");
                
            }

              _logger.LogInformation($"Admin Security Settings with ID { deleteAdminSecuritySettingsCommand.Id} found. Proceeding with deletion.");

            // Map request to entity and delete
            var updatedAdminSecuritySettings = _Imapper.Map<Core.Domain.Entities.AdminSecuritySettings>(deleteAdminSecuritySettingsCommand);
            var result = await _IadminSecuritySettingsCommandRepository.DeleteAsync(deleteAdminSecuritySettingsCommand.Id, updatedAdminSecuritySettings);

            if (result <= 0)
            {
                _logger.LogWarning($"Failed to delete Admin Security Settings with ID { deleteAdminSecuritySettingsCommand.Id}.");
                return result;
            }

            _logger.LogInformation($"Admin Security Settings with ID { deleteAdminSecuritySettingsCommand.Id} deleted successfully.");

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "",
                actionName: "",
                details: $"Admin Security Settings ID: {deleteAdminSecuritySettingsCommand.Id} was changed to status inactive.",
                module: "Admin Security Settings"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for Admin Security Settings ID { deleteAdminSecuritySettingsCommand.Id}.");

            return result;  
   
      }

    }
}