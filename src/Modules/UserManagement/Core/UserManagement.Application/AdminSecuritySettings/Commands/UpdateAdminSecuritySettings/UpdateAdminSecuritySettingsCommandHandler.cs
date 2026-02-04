using AutoMapper;
using UserManagement.Application.Common;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Domain.Events;
using MediatR;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings
{
    public class UpdateAdminSecuritySettingsCommandHandler      : IRequestHandler<UpdateAdminSecuritySettingsCommand ,int>
    {
       public readonly IAdminSecuritySettingsCommandRepository _IAdminSecuritySettingsCommandRepository;
       private readonly IMapper _Imapper; 
        
        private readonly IAdminSecuritySettingsQueryRepository _IAdminSecuritySettingsQueryRepository;
        private readonly IMediator _mediator; 
         private readonly ILogger<UpdateAdminSecuritySettingsCommandHandler> _logger;

          public UpdateAdminSecuritySettingsCommandHandler(IAdminSecuritySettingsCommandRepository adminSecuritySettingsCommandRepository,IAdminSecuritySettingsQueryRepository adminSecuritySettingsQueryRepository , IMapper Imapper, IMediator mediator ,ILogger<UpdateAdminSecuritySettingsCommandHandler> logger)
        {
            _IAdminSecuritySettingsCommandRepository = adminSecuritySettingsCommandRepository;
            _IAdminSecuritySettingsQueryRepository = adminSecuritySettingsQueryRepository;
            _Imapper = Imapper;
            _logger = logger;           
             _mediator = mediator;
        }

        public async Task<int> Handle(UpdateAdminSecuritySettingsCommand request, CancellationToken cancellationToken)
        {
             // Fetch the existing adminSecuritySettings by ID
                var adminSecSettings  = await _IAdminSecuritySettingsQueryRepository.GetAdminSecuritySettingsByIdAsync(request.Id);
      
                if (adminSecSettings  is null )
                {
                 
                   _logger.LogWarning($" Admin Settings with ID {request.Id} not found.");
                   throw new ValidationException("AdminSecuritySettings not found/AdminSecuritySettings is deleted ");
             
                }                
              var adminsettingsentity = _Imapper.Map<UserManagement.Domain.Entities.AdminSecuritySettings>(request);

              var result = await _IAdminSecuritySettingsCommandRepository.UpdateAsync(request.Id, adminsettingsentity);
            
             if (result == -1) // Entity not found
            {
                _logger.LogWarning($"Failed to update AdminSecuritySettings with ID {request.Id }.");
                throw new Exception("Failed to update AdminSecuritySettings");
               
            }

            _logger.LogInformation($"AdminSecuritySettings with ID { request.Id} updated successfully.");

            // Map the updated entity to DTO
            var adminsetting = await _IAdminSecuritySettingsQueryRepository.GetAdminSecuritySettingsByIdAsync(request.Id);
            var adminsettingsDto = _Imapper.Map<AdminSecuritySettingsDto>(adminsetting);

         
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: $"Update Admin Security Settings{adminsettingsDto.Id}" ,
                     actionName: "Update Admin Security Settings",
                    details: $"Admin Settings  was updated. Admin Settings ID: {request.Id}",
                    module: "Admin Secutrity Settings"
                );
               
                 await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for AdminSecuritySettings ID {adminSecSettings.Id}.");

            return result;

        
        }


        
    }
}