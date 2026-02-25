using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRole;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserRole.Commands.DeleteRole
{
    public class DeleteRoleCommandHandler :IRequestHandler<DeleteRoleCommand ,int>
  
    {
    
        private readonly IUserRoleCommandRepository _IuserroleRepository;  
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IUserRoleQueryRepository _userRoleQueryRepository;
         private readonly ILogger<DeleteRoleCommandHandler> _logger;
      
      public DeleteRoleCommandHandler (IUserRoleCommandRepository roleRepository , IMapper mapper,ILogger<DeleteRoleCommandHandler> logger,IMediator mediator, IUserRoleQueryRepository userRoleQueryRepository)
      {
        _IuserroleRepository =roleRepository;
         _mapper = mapper;
         _logger = logger;
         _mediator = mediator;
         _userRoleQueryRepository = userRoleQueryRepository;
      }

       public async Task<int>Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
      {       
         _logger.LogInformation($"DeleteUserroleCommandHandler started for User Role ID: {request.Id}");
            var updateduserrolemap = _mapper.Map<UserManagement.Domain.Entities.UserRole>(request);
           
            _logger.LogInformation($"User Role  with ID {request.Id} found. Proceeding with deletion.");

            
           var userrole = await _IuserroleRepository.DeleteAsync(request.Id, updateduserrolemap);
                         
            if (userrole <= 0)
            {
                _logger.LogWarning($"Failed to delete UserRole   with ID {request.Id}.");
                throw new Exception("Failed to delete UserRole");
              
            }
                _logger.LogInformation($"UserRole with ID {request.Id} deleted successfully.");

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: updateduserrolemap.Id.ToString(),
                actionName: "",
                details: $"UserRole ID: {request.Id} was changed to status inactive.",
                module: "UserRole"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for UserRole ID {request.Id}." );

            return userrole;  
           
      }


    }
}