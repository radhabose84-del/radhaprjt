using AutoMapper;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces.IAdminSecuritySettings;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById
{
    public class GetAdminSecuritySettingsByIdQueryHandler :IRequestHandler<GetAdminSecuritySettingsByIdQuery, GetAdminSecuritySettingsDto>
    {    
          private readonly IAdminSecuritySettingsQueryRepository _IAdminSecuritySettingsQueryRepository;        
        private readonly IMapper _mapper;
         private readonly IMediator _mediator;
          private readonly ILogger<GetAdminSecuritySettingsByIdQueryHandler> _logger;    

    public GetAdminSecuritySettingsByIdQueryHandler(IAdminSecuritySettingsQueryRepository  adminSecuritySettingsQueryRepository,IMapper mapper , IMediator mediator,ILogger<GetAdminSecuritySettingsByIdQueryHandler> logger)
         {
            _IAdminSecuritySettingsQueryRepository = adminSecuritySettingsQueryRepository;
            _mapper =mapper;
            _mediator = mediator;
            _logger = logger;
        } 

           public async Task<GetAdminSecuritySettingsDto> Handle(GetAdminSecuritySettingsByIdQuery request, CancellationToken cancellationToken)
        {
             _logger.LogInformation($"Handling GetAdminSecuritySettingsByIdQuery for ID: { request.Id}");
            // Fetch admin security setting by ID
                var adminSettings = await _IAdminSecuritySettingsQueryRepository.GetAdminSecuritySettingsByIdAsync(request.Id);
                 if (adminSettings is null)
                    {
                        _logger.LogWarning($"AdminSecuritySettings with ID {request.Id} not found." );
                        throw new ValidationException("AdminSecuritySettings not found.");
                    
                    }
        
                _logger.LogInformation($"Admin Security Settings with ID {request.Id} retrieved successfully. Mapping to DTO.");

                // Map to DTO
                var adminSettingsDto = _mapper.Map<GetAdminSecuritySettingsDto>(adminSettings);
                // Publish domain event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "GetById",
                    actionName: adminSettingsDto.Id.ToString(),
                    details: $"Admin Security Setting with ID '{adminSettingsDto.Id}' was fetched.",
                    module: "Admin Security Settings"
                );

                _logger.LogInformation($"AuditLogsDomainEvent published for Admin Security Settings with ID {request.Id}.");

             
                   await _mediator.Publish(domainEvent, cancellationToken);
                  return adminSettingsDto;
        

          
        }


    }
}