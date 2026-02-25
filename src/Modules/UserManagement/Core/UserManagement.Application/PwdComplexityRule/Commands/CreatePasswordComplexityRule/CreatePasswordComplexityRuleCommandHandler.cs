#nullable disable
using AutoMapper;
using UserManagement.Application.PwdComplexityRule.Queries;
using MediatR;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.PwdComplexityRule.Commands.CreatePasswordComplexityRule
{
    public class CreatePasswordComplexityRuleCommandHandler  :IRequestHandler<CreatePasswordComplexityRuleCommand , PwdRuleDto>

    {
          private readonly IPasswordComplexityRuleCommandRepository _passwordComplexityRepository;
           private readonly IMapper _mapper;
           private readonly IMediator _mediator; 
           private readonly ILogger<CreatePasswordComplexityRuleCommandHandler> _logger;
           public CreatePasswordComplexityRuleCommandHandler(IPasswordComplexityRuleCommandRepository passwordComplexityRepository ,IMapper mapper,IMediator mediator,ILogger<CreatePasswordComplexityRuleCommandHandler> logger)
        {
             _passwordComplexityRepository=passwordComplexityRepository;
               _mapper=mapper;
               _mediator=mediator;
               _logger=logger;
         
        }
         public async Task<PwdRuleDto> Handle(CreatePasswordComplexityRuleCommand request, CancellationToken cancellationToken)
        {         
          _logger.LogInformation($"Handling CreatePasswordComplexityRuleCommand for Password Complexity Rule: {request.PwdComplexityRule}");
     
                var exists = await _passwordComplexityRepository.ExistsByCodeAsync(request.PwdComplexityRule);
                    if (exists)
                    {
                       _logger.LogWarning($"PasswordComplexityRule {request.PwdComplexityRule} already exists" );
                       throw new ValidationException("Password Complexity Rule Name already exists.");
                     
                    }
                var passwordComplexityRuleEntity = _mapper.Map<UserManagement.Domain.Entities.PasswordComplexityRule>(request);
                var result = await _passwordComplexityRepository.CreateAsync(passwordComplexityRuleEntity);

                if (result is null)
            {
                _logger.LogWarning($"Failed to create Password Complexity Rule. Password Complexity Rule entity: {passwordComplexityRuleEntity}");
                throw new ValidationException("Password Complexity Rule not created");
            
            }

                _logger.LogInformation($"Password Complexity Rule created successfully with ID: { result.Id}");

            
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: result.Id.ToString(),
                    actionName: result.PwdComplexityRule,
                    details: $"Password Complexity Rule '{result.PwdComplexityRule}' was created. Rule ID: {result.Id}",
                    module: "PasswordComplexityRule"
                );


                  await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for Department ID: {result.Id}");

        
            var ruleDto = _mapper.Map<PwdRuleDto>(result);

            _logger.LogInformation($"Returning success response for Department ID: {result.Id}");

            return ruleDto;
        }

    }
}