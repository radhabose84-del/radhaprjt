#nullable disable
using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;


namespace UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule
{
    public class UpdatePasswordComplexityRuleCommandHandler :IRequestHandler<UpdatePasswordComplexityRuleCommand, bool>
    {
         public readonly IPasswordComplexityRuleCommandRepository  _IPasswordComplexityRepository;
         private readonly IMapper _Imapper;  
        private readonly IPasswordComplexityRuleQueryRepository _IpasswordComplexityRuleQueryRepository;
        private readonly IMediator _mediator; 
            private readonly ILogger<UpdatePasswordComplexityRuleCommandHandler> _logger;
         
          public UpdatePasswordComplexityRuleCommandHandler(IPasswordComplexityRuleCommandRepository passwordComplexityRepository,IPasswordComplexityRuleQueryRepository passwordComplexityRuleQueryRepository, IMapper mapper,IMediator mediator,ILogger<UpdatePasswordComplexityRuleCommandHandler> logger)
          {
              _IPasswordComplexityRepository = passwordComplexityRepository;
             
            _IpasswordComplexityRuleQueryRepository = passwordComplexityRuleQueryRepository;
              _Imapper = mapper;
              _mediator = mediator;
              _logger = logger;

          }

        public async Task<bool> Handle(UpdatePasswordComplexityRuleCommand request, CancellationToken cancellationToken)
            {
              _logger.LogInformation($"Handling UpdatePasswordComplexityRuleCommand for Password Complexity Rule with ID: { request.Id}");


                 var dePasswordComplexityMap  = _Imapper.Map<UserManagement.Domain.Entities.PasswordComplexityRule>(request);
                  
                  _logger.LogInformation($"Password Complexity Rule with ID {request.Id} retrieved successfully. Proceeding with update." );
   
                    // Save the updates
                    var result = await _IPasswordComplexityRepository.UpdateAsync(request.Id, dePasswordComplexityMap);
                                if (result <=0)
                        {
                            _logger.LogWarning($"Failed to update Password Complexity Rule with ID {request.Id}.");
                            throw new ValidationException("Failed to update Password Complexity Rule");
                       
                        }
                  

                    _logger.LogInformation($"Password Complexity Rule with ID {request.Id} updated successfully.");

                    // Publish domain event
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: dePasswordComplexityMap.Id.ToString(),
                        actionName: dePasswordComplexityMap.PwdComplexityRule,
                        details: $"Password Complexity Rule '{dePasswordComplexityMap.PwdComplexityRule}' was updated. Password Complexity Rule ID: {request.Id}",
                        module: "PasswordComplexityRule"
                    );

                 
                      await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for Password Complexity Rule ID {request.Id}.");

            return result > 0;                                     

            }


 
        
    }
}