using AutoMapper;
using Core.Application.Common.Interfaces;
using MediatR;
using System.Text;
using Core.Domain.Entities;
using Core.Application.Users.Queries.GetUsers;
using Core.Application.Common.Interfaces.IUser;
using Core.Domain.Events;
using Microsoft.Extensions.Logging;
using Core.Application.Common.HttpResponse;
namespace Core.Application.Users.Queries.GetUserAutoComplete
{
    public class GetUserAutoCompleteQueryHandler: IRequestHandler<GetUserAutoCompleteQuery,ApiResponseDTO<List<UserAutoCompleteDto>>>
    {

        private readonly IUserQueryRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ILogger<GetUserAutoCompleteQueryHandler> _logger;


		public GetUserAutoCompleteQueryHandler(IUserQueryRepository userRepository, IMapper mapper, IMediator mediator,ILogger<GetUserAutoCompleteQueryHandler> logger)
        {
           _userRepository =userRepository;
           _mapper =mapper;
           _mediator = mediator;
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           
        }  
        
       public async Task<ApiResponseDTO<List<UserAutoCompleteDto>>> Handle(GetUserAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            
            
            _logger.LogInformation("Fetching users matching SearchPattern: {SearchPattern}", request.SearchPattern);

            // Fetch users matching the search pattern
            var users = await _userRepository.GetUser(request.SearchPattern);
           

            _logger.LogInformation("Found {UserCount} users matching SearchPattern: {SearchPattern}", users.Count, request.SearchPattern);
            // Publish a domain event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode:"",        
                    actionName: request.SearchPattern,                
                    details: $"User '{request.SearchPattern}' was searched",
                    module:"User"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                
                var userDto = _mapper.Map<List<UserAutoCompleteDto>>(users);
            return new ApiResponseDTO<List<UserAutoCompleteDto>>
            {
                IsSuccess = true,
                Data = userDto
            };
            
        }
    }
}