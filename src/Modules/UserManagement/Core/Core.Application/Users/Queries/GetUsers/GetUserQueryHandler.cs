using AutoMapper;
using MediatR;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces.IUser;
using Core.Domain.Events;
using Microsoft.Extensions.Logging;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Users.Queries.GetUsers
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery,ApiResponseDTO<List<UserDto>>>
    {
        private readonly IUserQueryRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ILogger<GetUserQueryHandler> _logger;


        public GetUserQueryHandler(IUserQueryRepository userRepository , IMapper mapper, IMediator mediator,ILogger<GetUserQueryHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        }
        public async Task<ApiResponseDTO<List<UserDto>>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all users from the repository.");
            
            // Fetch all users from the repository
            var (users, totalCount) = await _userRepository.GetAllUsersAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            if (users == null || users.Count == 0)
            {
                _logger.LogWarning("No users found in the repository.");
                return new ApiResponseDTO<List<UserDto>>
                { 
                    IsSuccess = false, 
                    Message = "No users found"
                };
            }

            var userList = _mapper.Map<List<UserDto>>(users);
            _logger.LogInformation("Fetched {UserCount} users from the repository.", users.Count);
            
            //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Fetch",
                    actionCode: "GetAllUsers",        
                    actionName: "Get Users",
                    details: $"Fetched details of {users.Count} users.",
                    module:"User"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<UserDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = userList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                
            };
        }
    }
}


