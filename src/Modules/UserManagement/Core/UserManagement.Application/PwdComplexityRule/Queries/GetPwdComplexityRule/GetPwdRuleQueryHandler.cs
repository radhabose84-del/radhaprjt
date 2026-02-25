using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IPasswordComplexityRule;
using Microsoft.Extensions.Logging;
using Contracts.Common;
using UserManagement.Domain.Events;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;

namespace UserManagement.Application.PwdComplexityRule.Queries
{
    public class GetPwdRuleQueryHandler  :IRequestHandler<GetPwdRuleQuery, ApiResponseDTO<List<GetPwdRuleDto>>>
    {
       private readonly IPasswordComplexityRuleQueryRepository _passwordComplexityRepository; 
       private readonly IMapper _mapper; 
      private readonly ILogger<GetPwdRuleQueryHandler> _logger;

        private readonly IMediator _mediator; 


        public GetPwdRuleQueryHandler( IPasswordComplexityRuleQueryRepository passwordComplexityRepository,IMapper mapper , ILogger<GetPwdRuleQueryHandler> logger,IMediator mediator )
        {
             _mapper =mapper;
            _passwordComplexityRepository = passwordComplexityRepository;  
            _logger = logger; 
            _mediator = mediator;
        }
    

        public async Task<ApiResponseDTO<List<GetPwdRuleDto>>> Handle(GetPwdRuleQuery request, CancellationToken cancellationToken)
        {

           _logger.LogInformation("Fetching Password Complexity Rule Request started: {request}", request);
           
           
             var (pwdcomplexityrules,totalCount) = await _passwordComplexityRepository.GetPasswordComplexityAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            
             if (pwdcomplexityrules is null )
            {
               _logger.LogWarning($"No Password Rule records found in the database. Total count: {pwdcomplexityrules?.Count ?? 0}" );

                  return new ApiResponseDTO<List<GetPwdRuleDto>> { IsSuccess = false, Message = "No Record Found" };
            }

             var pwdcomruleList = _mapper.Map<List<GetPwdRuleDto>>(pwdcomplexityrules);
             var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"Password Complexity Rule details was fetched.",
                    module:"Password Complexity Rule"
                );

                  await _mediator.Publish(domainEvent, cancellationToken);
              
            _logger.LogInformation($"Password Complexity Rule { pwdcomruleList.Count}  Listed successfully.");
            return new ApiResponseDTO<List<GetPwdRuleDto>> 
            {
                 IsSuccess = true, 
                 Message = "Success", 
                 Data = pwdcomruleList ,
                  TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                 };  

        
     

        }



        
    }

    

}