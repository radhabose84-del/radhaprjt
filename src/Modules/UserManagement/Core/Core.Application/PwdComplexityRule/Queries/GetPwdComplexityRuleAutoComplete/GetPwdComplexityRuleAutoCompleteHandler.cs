using Core.Application.Common.Interfaces;
using Core.Domain.Entities;
using MediatR;
using AutoMapper;
using Core.Application.PwdComplexityRule.Queries;
using Core.Application.Common.Interfaces.IPasswordComplexityRule;
using Core.Application.Common;
using Core.Domain.Events;
using Microsoft.Extensions.Logging;
using Core.Application.Common.HttpResponse;
using Core.Application.UserRole.Queries.GetRole;


namespace Core.Application.PwdComplexityRule.Queries.GetPwdComplexityRuleAutoComplete
{
    public class GetPwdComplexityRuleAutoCompleteHandler : IRequestHandler<GetPwdComplexityRuleAutoComplete, List<PwdComplexityRuleAutoCompleteDto>>
    {              
        private readonly IPasswordComplexityRuleCommandRepository _passwordComplexityRuleCommandRepository;
        private readonly IPasswordComplexityRuleQueryRepository _passwordComplexityRuleQueryRepository;             
        private readonly IMapper _mapper;
        private readonly ILogger<GetPwdComplexityRuleAutoCompleteHandler> _logger;
        private readonly IMediator _mediator;


    public GetPwdComplexityRuleAutoCompleteHandler(IPasswordComplexityRuleCommandRepository passwordComplexityRuleCommandRepository,IPasswordComplexityRuleQueryRepository passwordComplexityRuleQueryRepository , IMapper mapper, IMediator mediator,ILogger<GetPwdComplexityRuleAutoCompleteHandler> logger)
        {
        _passwordComplexityRuleCommandRepository=passwordComplexityRuleCommandRepository;
        _passwordComplexityRuleQueryRepository=passwordComplexityRuleQueryRepository;
            _mapper =mapper;
            _mediator=mediator;
            _logger = logger;
        }

        public async Task<List<PwdComplexityRuleAutoCompleteDto>> Handle(GetPwdComplexityRuleAutoComplete request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handling GetPwdComplexityRuleAutoCompleteQuery with search pattern: {request.SearchTerm}");

            // Fetch password complexity rules matching the search pattern
            var pwdComplexityRules = await _passwordComplexityRuleQueryRepository.GetpwdautocompleteAsync(request.SearchTerm);

        

            _logger.LogInformation($"Password complexity rules found for search pattern: {request.SearchTerm}. Mapping results to DTO.");

            // Map the result to DTO
            var pwdRuleDtoList = _mapper.Map<List<PwdComplexityRuleAutoCompleteDto>>(pwdComplexityRules);

            // Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode: "",
                actionName: request.SearchTerm,
                details: $"Password Complexity Rule '{request.SearchTerm}' was searched",
                module: "Password Complexity Rule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Domain event published for search pattern: {request.SearchTerm}");

            return  pwdRuleDtoList;
        }

        


    }
}