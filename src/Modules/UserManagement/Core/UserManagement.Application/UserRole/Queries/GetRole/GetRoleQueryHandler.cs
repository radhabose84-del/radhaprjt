
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserRole;
using Contracts.Common;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;


namespace UserManagement.Application.UserRole.Queries.GetRole
{
    public class GetRoleQueryHandler :IRequestHandler<GetRoleQuery,ApiResponseDTO<List<GetUserRoleDto>>>
    {
       
     private readonly IUserRoleQueryRepository _userRoleRepository;
     private readonly IMapper _mapper;
     
       private readonly IMediator _mediator; 
         private readonly ILogger<GetRoleQueryHandler> _logger;
   


       public GetRoleQueryHandler(IUserRoleQueryRepository userRoleRepository, IMapper mapper, IMediator mediator, ILogger<GetRoleQueryHandler> logger)
        {
            _userRoleRepository = userRoleRepository;
            _mapper =mapper;
            _mediator = mediator;
            _logger = logger;
        }
            public async Task<ApiResponseDTO<List<GetUserRoleDto>>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching User Role Request started: {request}", request);

                // Fetch user roles with pagination and search
                var (roles, totalCount) = await _userRoleRepository.GetAllRoleAsync(request.PageNumber, request.PageSize, request.SearchTerm);

                if (roles is null || !roles.Any())
                {
                    _logger.LogWarning("No User Role records found in the database. Total count: {totalCount}", totalCount);

                    return new ApiResponseDTO<List<GetUserRoleDto>>
                    {
                        IsSuccess = false,
                        Message = "No Record Found"
                    };
                }

                _logger.LogInformation("Mapping user roles to DTO.");
                var roleList = _mapper.Map<List<GetUserRoleDto>>(roles);

                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",
                    actionName: "",
                    details: "User Role details were fetched.",
                    module: "UserRole"
                );

                _logger.LogInformation("Publishing AuditLogsDomainEvent.");
                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogInformation("User Role records listed successfully. Count: {Count}", roleList.Count);

                return new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = roleList,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

     


  }
}